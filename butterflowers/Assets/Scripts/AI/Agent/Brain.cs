﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI.Types;
using AI.Types.Mappings;
using AI.UI;
using Data;
using Noder.Graphs;
using Noder.Nodes.Behaviours.Entries;
using Objects.Managers;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uwu;
using uwu.Camera;
using uwu.Extensions;
using Action = AI.Types.Action;
using Behaviour = AI.Types.Behaviour;
using Random = UnityEngine.Random;

namespace AI.Agent
{
	public class Brain : MonoBehaviour
	{
		// External

		GameDataSaveSystem Save;
		Library Library;
		Sun Sun;
		World World;

		public BeaconManager Beacons;
		public Nest Nest;
		public ButterflowerManager Butterflowers;
		public Focusing Focus;
		
		// Properties

		[SerializeField] WorldPreset worldPreset;
		[SerializeField] Surveillance surveillance;
		
		[SerializeField] Scriptables.BrainPreset preset;

		[SerializeField] BehaviourProfile cacheProfile;
		[SerializeField] BehaviourProfile m_profile;

		[SerializeField] BehaviourFloatGroup profileDelta = new BehaviourFloatGroup();
		bool needsToCalculateDelta = false;

		[SerializeField] ModuleTree[] behaviourTrees;
		[SerializeField] ModuleTreeHelper behaviourTreeHelper;

		Actions actions;
		
		[SerializeField] 
		Body body;
		
		// Collections

		AttributeElement[] attributeObjects;
		StatElement[] statObjects;
		
		Dictionary<Behaviour, AttributeElement> hudAttributes = new Dictionary<Behaviour, AttributeElement>();
		Dictionary<Behaviour, StatElement> hudStats = new Dictionary<Behaviour, StatElement>();

		// Attributes

		[SerializeField] bool load = false, active = false, reset = true, deltas = false;

		[Header("State")] 
			[SerializeField] [Range(-1f, 1f)] float m_mood = 0f;
			[SerializeField] [Range(-1f, 1f)] float t_mood = 0f;
			[SerializeField] [Range(0f, 1f)] float m_stance = 0f;
			
			[SerializeField] Mood m_moodState = Mood.NULL;
			[SerializeField] SecondaryMood m_submoodState = SecondaryMood.NULL;
			[SerializeField] Stance m_stanceState = Stance.NULL;
		
		// Collections

		[Header("Needs")]
			[SerializeField] BehaviourFloatGroup needs = new BehaviourFloatGroup();
			[SerializeField] ModuleTree bestModuleTree = null;
			[SerializeField] List<ModuleTree> bestModuleTrees = new List<ModuleTree>();

		[Header("Knowledge")]
			[SerializeField] float m_enviroKnowledge = 0f;
			[SerializeField] Knowledge[] m_fileKnowledge = null;
				Dictionary<string, Knowledge> fileKnowledgeLookup = new Dictionary<string, Knowledge>();
		
		[Header("Events")]	
			
			public List<EVENTCODE> PLAYER_EVENTS = new List<EVENTCODE>();
			int RECEIVE_PLAYER_EVENT = -1;

			public List<EVENTCODE> WORLD_EVENTS = new List<EVENTCODE>();
			int RECEIVE_WORLD_EVENT = -1;

			public List<EVENTCODE> NEST_EVENTS = new List<EVENTCODE>();
			int RECEIVE_NEST_EVENT = -1;

		[Header("UI")] 
			
			[SerializeField] Transform hudAttributesRoot, hudStatsRoot;
			[SerializeField] GameObject hudAttributePrefab, hudStatPrefab;
		
			[SerializeField] BehaviourStringGroup hudIdentifiers = new BehaviourStringGroup();
			[SerializeField] BehaviourSpriteGroup hudIcons = new BehaviourSpriteGroup(); 
			
			[SerializeField] int maxHudStatScore = 10;
			[SerializeField] int[] deltaStats = new int[] { };

		#region Accessors

		public bool isActive => active;
		
		public bool Self => (active && World.Parallel);
		public bool Remote => (active && World.Remote);

		public Actions Actions => actions;
		public Body Body => body;

		public BehaviourProfile Profile
		{
			get { return m_profile; }
			set { m_profile = value;  }
		}

		public float environmentKnowledge => m_enviroKnowledge;

		public BrainData brainnnnnnnn;

		public Knowledge[] fileKnowledge
		{
			get { return m_fileKnowledge;  }
			set { m_fileKnowledge = value; }
		}
		
		public float mood => m_mood;
		public float stance => m_stance;

		public Stance Stance => m_stanceState;
		public Mood Mood => m_moodState;
		public SecondaryMood SecondaryMood => m_submoodState;
		
		int EVENT_WAIT => preset.eventNullFramesThreshold;
		int EVENT_STACK_HEIGHT => preset.eventMaximumStackHeight;
		
		public EVENTCODE LAST_PLAYER_EVENT => (PLAYER_EVENTS.Count > 0) ? PLAYER_EVENTS.Last() : EVENTCODE.NULL;
		public EVENTCODE LAST_WORLD_EVENT => (WORLD_EVENTS.Count > 0) ? WORLD_EVENTS.Last() : EVENTCODE.NULL;
		public EVENTCODE LAST_NEST_EVENT => (NEST_EVENTS.Count > 0) ? NEST_EVENTS.Last() : EVENTCODE.NULL;
		
		#endregion
		
		
		#region Monobehaviour callbacks

		void Awake()
		{
			actions = GetComponent<Actions>();
		}

		void OnEnable()
		{
			Actions.onCompleteAction += onCompletedAction;
			Actions.onFailAction += onFailAction;
			
			Events.onFireEvent += onFireEvent;
			
			ModuleTree.onReceiveEvent += OnReceiveEventFromTree;
			ModuleTree.onFailEvent += OnFailEventFromTree;
			ModuleTree.onReceiveRewards += OnReceiveRewardsFromTree;

			body.onTeleport += Fatigue;

			StartCoroutine("ActionLoop");
			StartCoroutine("EventLoop");
		}

		void OnDisable()
		{
			Actions.onCompleteAction -= onCompletedAction;
			Actions.onFailAction -= onFailAction;
			
			body.onTeleport -= Fatigue;
			
			Events.onFireEvent -= onFireEvent;
			
			ModuleTree.onReceiveEvent -= OnReceiveEventFromTree;
			ModuleTree.onFailEvent -= OnFailEventFromTree;
			ModuleTree.onReceiveRewards -= OnReceiveRewardsFromTree;
			
			StopAllCoroutines();
		}

		void Start()
		{
			World = World.Instance;

			BindUIElements();
			
			RECEIVE_PLAYER_EVENT = RECEIVE_WORLD_EVENT = RECEIVE_NEST_EVENT = -EVENT_WAIT;

			foreach (ModuleTree tree in behaviourTrees)
				tree.Brain = behaviourTreeHelper;
		}
		
		void Update()
		{
			if (!load || !Sun.active)
				return;
			
			active = (World.state != World.State.User);
			
			if (!active) 
			{
				reset = true;
			}
			else 
			{
				if (reset) 
				{
					Reload();
					reset = false;
				}
			}

			KnowledgeUpdate();

			if (active) 
			{
				ActionUpdate();
				StateUpdate();
				
				if(World.state != World.State.User)
					VisualUpdate(); // Update HUD when user is not active

				cacheProfile = Profile; // Wipe cache profiel
			}
		}

		void OnDestroy()
		{
			Dispose();
		}
		
		#endregion
		
		#region Initialization

		public void Load()
		{
			Save = GameDataSaveSystem.Instance;
			Library = Library.Instance;
			Sun = Sun.Instance;
			World = World.Instance;

			m_enviroKnowledge = (worldPreset.persistKnowledge) ? Save.enviro_knowledge : 0f;
			m_fileKnowledge = (worldPreset.persistKnowledge) ? Save.file_knowledge : new Knowledge[] { };

			PopulateKnowledgeLookup(m_fileKnowledge);

			active = (World.state != World.State.User);
			if(active)
				Reload(); // Load profile + needs

			bool success = Save.LoadGameData<BrainData>("brain.fns", createIfEmpty: true);
			brainnnnnnnn = Save.brainData;

			load = true;
		}

		public void Reload()
		{
			cacheProfile = Save.brainData.behaviourProfile;

			if (World.state == World.State.Parallel) 
				m_profile = Save.brainData.behaviourProfile;
			else 
			{
				if (!Save.brainData.load) 
				{
					m_profile = ConstructBehaviourProfile(); 
				}
				else
					m_profile = Save.brainData.behaviourProfile;

				Save.brainData.behaviourProfile = m_profile; // Assign profile to save file
			}

			RecalculateNeeds();
			profileDelta = CalculateDeltaProfile();
		}
		
		void Dispose()
		{
			fileKnowledgeLookup.Clear();
		}
		
		#endregion
		

		#region Core loops
		
		IEnumerator EventLoop()
		{
			while (true) {
				
				RECEIVE_PLAYER_EVENT++;
				RECEIVE_WORLD_EVENT++;
				RECEIVE_NEST_EVENT++;

				yield return new WaitForEndOfFrame();

				if (RECEIVE_PLAYER_EVENT > 0) {
					AddEventToQueue(EVENTCODE.NULL, ref PLAYER_EVENTS);
					RECEIVE_PLAYER_EVENT = -EVENT_WAIT;
				}
				if (RECEIVE_WORLD_EVENT > 0) {
					AddEventToQueue(EVENTCODE.NULL, ref WORLD_EVENTS);
					RECEIVE_WORLD_EVENT = -EVENT_WAIT;
				}
				if (RECEIVE_NEST_EVENT > 0) {
					AddEventToQueue(EVENTCODE.NULL, ref NEST_EVENTS);
					RECEIVE_NEST_EVENT = -EVENT_WAIT;
				}

				yield return null;
			}
		}

		IEnumerator ActionLoop()
		{
			while (true) 
			{
				if (!Actions.available && active) // Wait till wand is available
				{
					var tree = bestModuleTree = CalculateBestAction();
					Debug.Log("best tree action= " + ((tree == null)? "NULL":tree.name));
					
					if (tree != null)
						tree.Restart(); // Restart tree to find relevant action
				}

				yield return new WaitForSeconds(1.3f);
			}
		}
		
		#endregion
		
		#region Core updates

		void KnowledgeUpdate()
		{
			if (Nest.open) // Only learn when nest is open
			{
				float dt = Time.deltaTime;

				LearnFromEnvironment(dt);
				LearnFromBeacons(dt);
			}

			fileKnowledge = ScrapeKnowledgeLookup();

			// Update save file
			Save.enviro_knowledge = environmentKnowledge;
			Save.file_knowledge = fileKnowledge;
		}

		void ActionUpdate()
		{
			DecayAllNeeds();
		}

		void StateUpdate()
		{
			StanceUpdate();
			m_stanceState = EvaluateStance();

			MoodUpdate();
			m_moodState = EvaluateMood();
			m_submoodState = EvaluateSecondaryMood();
		}

		void VisualUpdate()
		{
			GetHUD();
		}
		
		#endregion


		#region Access states

		public Mood getMoodState()
		{
			var state = Mood.Neutral;

			if (mood >= preset.highMoodThreshold)
				state = Mood.Joyous;
			if (mood <= preset.lowMoodThreshold)
				state = Mood.Depression;

			return state;
		}

		Mood getMoodFromAttribute(float value)
		{
			if (value >= preset.highMoodThreshold)
				return Mood.Joyous;
			if (value <= preset.lowMoodThreshold)
				return Mood.Depression;

			return Mood.Neutral;
		}

		public SecondaryMood getSubMoodState()
		{
			var mood = this.mood;
			var state = getMoodState();

			if (state == Mood.Depression) 
			{
				var thresh = preset.lowMoodThreshold;
				var max = Mathf.Abs(-1f - mood);
				var diff = Mathf.Abs(mood - thresh);

				if (diff / max >= .5f)
					return SecondaryMood.Violence;
				else
					return SecondaryMood.Mischief;
			}
			else if (state == Mood.Joyous) 
			{
				var thresh = preset.highMoodThreshold;
				var max = Mathf.Abs(1f - mood);
				var diff = Mathf.Abs(mood - thresh);

				if (diff / max >= .5f)
					return SecondaryMood.Elation;
				else
					return SecondaryMood.Happy;
			}

			return SecondaryMood.NULL;
		}


		public Stance getStanceState(AGENT agent = AGENT.NULL)
		{
			float stance = this.stance;

			if (agent == AGENT.World)
				stance = FetchKnowledgeFromEnvironment(); // Pull down enviro knowledge instead

			var state = Stance.Neutral;

			if (stance >= preset.highStanceThreshold)
				state = Stance.Confidence;
			if (stance <= preset.lowStanceThreshold)
				state = Stance.Unknown;

			return state;
		}
		
		#endregion
		
		#region Evaluate state

		void StanceUpdate()
		{
			float EW = preset.enviroKnowledgeStanceWeight;
			float FW = preset.fileKnowledgeStanceWeight;
			float TW = (EW + FW);

			EW /= TW;
			FW /= TW;

			float enviro = FetchKnowledgeFromEnvironment();
			float files = FetchKnowledgeFromFiles();

			m_stance = (EW * enviro) + (FW * files);
		}

		Stance EvaluateStance(AGENT agent = AGENT.NULL)
		{
			float stance = this.stance;
			if (agent == AGENT.World)
				stance = FetchKnowledgeFromEnvironment(); // Pull down enviro knowledge instead

			var state = Stance.Neutral;

			if (stance >= preset.highStanceThreshold)
				state = Stance.Confidence;
			if (stance <= preset.lowStanceThreshold)
				state = Stance.Unknown;

			return state;
		}

		//todo Primarily factor the weight of varying needs -> leads to more volatile energy based in mood
		void MoodUpdate()
		{
			float SW = preset.stanceMoodWeight;
			float BHW = preset.healthOfButterflowersMoodWeight;
			float NW = preset.needsWeight;
			float TW = (SW + BHW + NW);

			SW /= TW;
			BHW /= TW;
			NW /= TW;
			
			float butterflowerhealth = Butterflowers.GetHealth();

			var allNeeds = needs.behaviours;
			var allNeedHealth = 0f;
			foreach (BehaviourFloat need in allNeeds) 
				allNeedHealth += (need.value / allNeeds.Count);

			float m = (SW * stance) + (BHW * butterflowerhealth) + (NW * allNeedHealth);
			t_mood = m.RemapNRB(0f, 1f, -1f, 1f, true);

			m_mood = Mathf.Lerp(m_mood, t_mood, Time.deltaTime * preset.moodSmoothSpeed); // Smooth mood towards target
		}

		Mood EvaluateMood()
		{
			var state = Mood.Neutral;

			if (mood >= preset.highMoodThreshold)
				state = Mood.Joyous;
			if (mood <= preset.lowMoodThreshold)
				state = Mood.Depression;

			return state;
		}

		SecondaryMood EvaluateSecondaryMood()
		{
			var state = Mood;

			if (state == Mood.Depression) 
			{
				var thresh = preset.lowMoodThreshold;
				var max = Mathf.Abs(-1f - mood);
				var diff = Mathf.Abs(mood - thresh);

				if (diff / max >= .5f)
					return SecondaryMood.Violence;
				else
					return SecondaryMood.Mischief;
			}
			else if (state == Mood.Joyous) 
			{
				var thresh = preset.highMoodThreshold;
				var max = Mathf.Abs(1f - mood);
				var diff = Mathf.Abs(mood - thresh);

				if (diff / max >= .5f)
					return SecondaryMood.Elation;
				else
					return SecondaryMood.Happy;
			}

			return SecondaryMood.NULL;
		}
		
		#endregion

		#region Nest safety check

		bool EnsureNestActive()
		{
			if (!Nest.open) 
			{
				Actions.Push(EVENTCODE.NESTKICK, null);
				return true;
			}

			return false;
		}

		#endregion
		
		#region Visibility

		public Focusable[] SortFocusesByAlignmentToTarget(Focusable[] focuses, Transform target)
		{
			var sorted = focuses.OrderByDescending(focus =>
			{
				var camera = (focus == null) ? Focus.FallbackCamera : (GameCamera)focus.camera;
				if (camera == null) return -99f;

				return camera.CalculateAlignmentWithTarget(target);
			});

			return sorted.ToArray();
		}
		
		#endregion
		

		#region Callbacks
		
		void OnReceiveEventFromTree(ModuleTree tree, EVENTCODE @event, object dat, BehaviourInt rewards)
		{
			if (!EnsureNestActive()) 
			{
				var eventcode = System.Enum.GetName(typeof(EVENTCODE), @event);
				if (eventcode.Contains("BEACON") || @event == EVENTCODE.NESTPOP)
					dat = Beacons.GetBeaconByFile((string) dat); // Override beacon file value with actual beacon
				
				Actions.Push(@event, dat, tree:tree); // Send event from tree to actions
				
				var re = tree.rewards; // Receive marginal reward from module tree
				if (re != null) {

					var behaviours = re.behaviours;
					foreach (BehaviourInt b in behaviours)
						ReceiveReward(b.behaviour, b.value);
				}
			}
		}

		void OnFailEventFromTree(ModuleTree tree, FailureCode err)
		{
			if (tree == bestModuleTree) 
			{
				bestModuleTrees.Remove(tree); // Remove tree from array
				bestModuleTree = null;
			}
		}

		void OnReceiveRewardsFromTree(ModuleTree tree, BehaviourInt rewards)
		{
			ReceiveReward(rewards.behaviour, rewards.value);
		}

		void onCompletedAction(Action action)
		{
			var rewards = action.rewards;
			if (rewards != null) {

				var behaviours = rewards.behaviours;
				foreach (BehaviourInt b in behaviours)
					ReceiveReward(b.behaviour, b.value);
			}

			bestModuleTrees = new List<ModuleTree>(); // Wipe sorted trees
			bestModuleTree = null;
		}

		void onFailAction(Action action)
		{
			var tree = action.tree;
			if (tree != null) 
			{
				if (tree == bestModuleTree) 
				{
					bestModuleTrees.Remove(tree); // Remove tree from array
					bestModuleTree = null;
				}
			}
		}
		
		#endregion
		

		#region Event queue

		void onFireEvent(EVENTCODE @event, AGENT a, AGENT b, string detail)
		{
			string event_name = System.Enum.GetName(typeof(EVENTCODE), @event);
			if (event_name.Contains("NEST")) {
				AddEventToQueue(@event, ref NEST_EVENTS);
				RECEIVE_NEST_EVENT = -EVENT_WAIT;
			}
			if (a == AGENT.User || a == AGENT.Inhabitants) {
				AddEventToQueue(@event, ref PLAYER_EVENTS);
				RECEIVE_PLAYER_EVENT = -EVENT_WAIT;
			}
			if (a == AGENT.World) {
				AddEventToQueue(@event, ref WORLD_EVENTS);
				RECEIVE_WORLD_EVENT = -EVENT_WAIT;
			}
		}

		void AddEventToQueue(EVENTCODE @event, ref List<EVENTCODE> arr)
		{
			if (arr.Count == EVENT_STACK_HEIGHT)
				arr.RemoveAt(0);

			arr.Add(@event);
		}

		#endregion
		

		#region Learning
		
		void LearnFromEnvironment(float dt)
		{
			m_enviroKnowledge += dt;
		}
		
		void LearnFromBeacons(float dt)
		{
			var beacons = Beacons.AllBeacons;
			if (beacons == null) return;

			for (int i = 0; i < beacons.Length; i++) {
				var beacon = beacons[i];

				var accel = Nest.HasBeacon(beacon);

				float multiplier = (accel) ? preset.nestLearningMultiplier : 1f;
				LearnFromSingleBeacon(beacon, dt, multiplier);
			}
		}
		
		void LearnFromSingleBeacon(Beacon beacon, float dt, float multiplier)
		{
			if (beacon == null) return;

			var file = beacon.file;
			if (!fileKnowledgeLookup.ContainsKey(file)) 
				AddKnowledgeToLookup(file);

			var k = fileKnowledgeLookup[file];
			k.AddTime(dt * multiplier);
		}


		void PopulateKnowledgeLookup(Knowledge[] knowledges)
		{
			fileKnowledgeLookup = new Dictionary<string, Knowledge>();
			
			if (knowledges != null) {
				for (int i = 0; i < knowledges.Length; i++) {
					var k = knowledges[i];
					var file = k.file;

					string file_name = null;
					Library.FetchFile(file, out file_name);

					if (file_name != null && !fileKnowledgeLookup.ContainsKey(file_name)) 
						fileKnowledgeLookup.Add(file_name, k);
				}
			}
		}

		Knowledge[] ScrapeKnowledgeLookup()
		{
			if (fileKnowledgeLookup != null) 
				return fileKnowledgeLookup.Values.ToArray();

			return null;
		}
		
		void AddKnowledgeToLookup(string file)
		{
			if (fileKnowledgeLookup.ContainsKey(file)) return;

			var ind = Library.FetchFileIndex(file);
			if (ind < 0) return;

			var k = new Knowledge();
				k.file = ind;
				k.time = 0f;

			fileKnowledgeLookup.Add(file, k);
		}
		
		#endregion
		
		#region Learning accessors

		public float FetchKnowledgeFromEnvironment()
		{
			var e = environmentKnowledge;
			float days_e = worldPreset.ConvertSecondsToDays(e);

			return Mathf.Clamp01(days_e / preset.daysUntilEnvironmentKnowledge);
		}
		
		public float FetchKnowledgeFromFiles() 
		{
			float files = 0f;

			var beacons = Beacons.AllBeacons;
			
			int files_len = beacons.Length;
			for (int i = 0; i < files_len; i++) {
				var beacon = beacons[i];
				float k = FetchKnowledgeFromBeacon(beacon);

				files += k;
			}
			files /= ((float)files_len);

			return files;
		}

		public float FetchKnowledgeFromBeacon(Beacon beacon)
		{
			var file = beacon.file;
			return FetchKnowledgeFromFile(file);
		}

		public float FetchKnowledgeFromFile(string file)
		{
			if (string.IsNullOrEmpty(file) || !fileKnowledgeLookup.ContainsKey(file)) return 0f;
			if (Library.IsShared(file) || Library.IsWizard(file)) return 1f;

			var k = fileKnowledgeLookup[file];
			float days_k = worldPreset.ConvertSecondsToDays(k.time);

			return Mathf.Clamp01(days_k / preset.daysUntilFileKnowledge);
		}

		#endregion
		
		
		#region Behaviour profile

		[SerializeField] CompositeSurveillanceData comp;
		BehaviourProfile ConstructBehaviourProfile()
		{
			var profile = new BehaviourProfile();
			
			var compositeLog = comp =  surveillance.CreateCompositeAverageLog(includeSelf:true);
			var logs = surveillance.allLogs;

			List<BehaviourFloat> maps = new List<BehaviourFloat>();

			var behaviours = System.Enum.GetValues(typeof(Behaviour));
			foreach (int behaviourVal in behaviours) {
				var b = (Behaviour) behaviourVal;
				
				BehaviourFloat map = new BehaviourFloat();
					map.behaviour = b;
					map.value = CalculateBehaviourWeightForProfile(b, compositeLog, logs);
					
				maps.Add(map);
			}
			
			var group = new BehaviourFloatGroup();
			group.behaviours = maps;

			profile.weights = group;
			return profile;
		}

		BehaviourFloatGroup CalculateDeltaProfile()
		{
			profileDelta = new BehaviourFloatGroup();

			var needs = this.needs.behaviours;
			foreach (BehaviourFloat need in needs) {
				var behaviour = need.behaviour;
				
				float old = 0f;
				try {
					old = cacheProfile.GetWeight(behaviour);
				}
				catch (System.Exception e) {
					old = 0f;
				}

				float current = Profile.GetWeight(behaviour);
				profileDelta.SetValue(behaviour, (current - old), true);
			}

			return profileDelta;
		}

		[SerializeField] SurveillanceDataDelta d;
		//todo Generate accurate behaviour profile from surveillance data
		float CalculateBehaviourWeightForProfile(Behaviour behaviour, CompositeSurveillanceData composite, SurveillanceData[] history)
		{
			SurveillanceDataDelta delta = d = new SurveillanceDataDelta(worldPreset.baselineSurveillanceData, composite);
			List<float> factors = new List<float>();
			
			switch (behaviour) {
				case Behaviour.PLAY:
					factors.AddRange(new float[] {
						delta.discoveries,
						delta.hob,
						delta.cursorspeed,
						delta.volatility,
						delta.nestKicks
					});

					break;
				case Behaviour.REST:
					factors.AddRange(new float[] {
						1f-delta.volatility,
						delta.beaconsPlanted,
						1f - delta.nestKicks
					});
					
					break;
				case Behaviour.NURTURE:
					factors.AddRange(new float[] {
						delta.beaconsPlanted,
						1f - delta.beaconsAdded,
						delta.hob,
						delta.nestfill,
						delta.filesAdded
					});
					
					break;
				case Behaviour.GLUTTONY:
					factors.AddRange(new float[] {
						delta.filesAdded,
						delta.nestfill,
						delta.beaconsAdded,
						delta.nestSpills
					});
					
					break;
				case Behaviour.SPONTANEITY:
					factors.AddRange(new float[] {
						delta.filesAdded,
						delta.filesRemoved,
						delta.discoveries,
						delta.cursorspeed,
						delta.volatility
					});
					
					break;
				case Behaviour.DESTRUCTION:
					factors.AddRange(new float[] {
						delta.filesRemoved,
						1f - delta.hob,
						1f - delta.nestfill,
						delta.cursorspeed,
						delta.beaconsAdded,
						delta.nestSpills
					});
					
					break;
			}

			var average = factors.Average() / preset.baselineDeltaPercentage;
			var avg = average.RemapNRB(-1f, 1f, 0f, 1f);

			return avg;
		}
		
		#endregion
		
		#region Needs 

		void RecalculateNeeds()
		{
			needs = new BehaviourFloatGroup();
			
			var behaviours = Profile.weights.behaviours;
			foreach (BehaviourFloat behaviour in behaviours)
				needs.SetValue(behaviour.behaviour, preset.maximumNeedsScore, true);
		}

		void DecayAllNeeds()
		{
			var behaviours = needs.behaviours;
			foreach (BehaviourFloat behaviour in behaviours) 
			{
				if (!preset.excludeDecayNeeds.Contains(behaviour.behaviour)) // Does not exclude from decay
				{
					var value = behaviour.value;
					var weight = Profile.GetDecay(behaviour.behaviour);
					var multiplier = (behaviour.behaviour != Behaviour.REST) ? preset.needDecayMultiplier : preset.recoveryMultiplier;

					value = Mathf.Clamp(value - Time.deltaTime * weight * multiplier, 0f, preset.maximumNeedsScore);

					needs.SetValue(behaviour.behaviour, value);
				}
			}
		}

		void Fatigue(float distanceTraveled)
		{
			float magnitude = distanceTraveled / preset.baseFatigueTeleportDistance;
			float weight = Profile.GetWeight(Behaviour.REST);

			float energy = needs.GetValue(Behaviour.REST);
			energy = Mathf.Max(energy - weight * magnitude * preset.maximumNeedsScore, 0f);
			
			needs.SetValue(Behaviour.REST, energy); // Set energy level from teleportation
		}
		
		#endregion
		
		#region Scoring and rewards

		ModuleTree CalculateBestAction()
		{
			if (bestModuleTrees.Count > 0) 
			{
				var tree = bestModuleTrees[0];
				bestModuleTrees.RemoveAt(0);

				return tree;
			}
			else {

				var advertisers = behaviourTrees;

				ModuleTree bestTree = null;

				List<ModuleTree> bestTrees = new List<ModuleTree>(advertisers);
				Dictionary<ModuleTree, float> bestTreeScores = new Dictionary<ModuleTree, float>();

				for (int i = 0; i < advertisers.Length; i++) 
				{
					var advertiser = advertisers[i];
					var bestScore = 0f;

					var behaviours = needs.behaviours;
					foreach (BehaviourFloat behaviour in behaviours) 
					{
						var need = behaviour.behaviour;
						var value = advertiser.rewards.GetValue(need);

						var sc = CalculateScoreForReward(need, value);
						if (sc > bestScore)
							bestScore = sc;
					}

					bestTreeScores.Add(advertiser, bestScore);
				}
				
				bestModuleTrees = new List<ModuleTree>(bestTrees.OrderByDescending(tree => bestTreeScores[tree])); // Assign sorted module trees
				bestTree = bestModuleTrees.First();
				
				bestTreeScores.Clear();
				
				return bestTree;
			}
		}

		float CalculateScoreForReward(Behaviour need, int reward)
		{
			int max = preset.maximumNeedsScore;
			
			int current = needs.HasValue(need)? Mathf.RoundToInt(needs.GetValue(need)) : preset.maximumNeedsScore;
			int future = Mathf.Clamp(current + reward, 0, max);

			float currentAtten = preset.needMultiplier / current;
			float futureAtten = preset.needMultiplier / future;

			return currentAtten - futureAtten;
		}
		
		void ReceiveReward(Behaviour need, int reward)
		{
			if (needs.HasValue(need)) {
				var current = needs.GetValue(need);
				current = Mathf.Clamp(current + (float) reward, 0f, preset.maximumNeedsScore);
				
				needs.SetValue(need, current);
			}
		}
		
		#endregion
		

		#region UI

		void BindUIElements()
		{
			attributeObjects = hudAttributesRoot.GetComponentsInChildren<AttributeElement>(); 
			statObjects = hudStatsRoot.GetComponentsInChildren<StatElement>();

			var behaviours = System.Enum.GetValues(typeof(Behaviour)) as Behaviour[];
			for (int i = 0; i < behaviours.Length; i++) 
			{
				var behaviour = behaviours[i];
				var icon = hudIcons.GetValue(behaviour);

				var attr = attributeObjects[i];
				var stat = statObjects[i];

				if (!hudAttributes.ContainsKey(behaviour) && attr.Behaviour == behaviour) 
				{
					attr.Icon.sprite = icon;
					hudAttributes.Add(behaviour, attr);
				}

				if (!hudStats.ContainsKey(behaviour) && stat.Behaviour == behaviour) 
				{
					stat.Icon.sprite = icon;
					hudStats.Add(behaviour, stat);
				}
			}
		}

		void UpdateHUDAttribute(Behaviour behaviour, Mood mood)
		{
			if (!hudAttributes.ContainsKey(behaviour)) return;

			AttributeElement el = hudAttributes[behaviour];

			Sprite icon = null;
			Color col = Color.white;
			
			preset.AssignMoodAttributeIcon(mood, out icon, out col);

			el.UpdateValue(icon);
			el.Value.color = col;
		}

		void UpdateHUDStat(Behaviour behaviour, int value, float percent, int delta = 0)
		{
			if (!hudStats.ContainsKey(behaviour)) return;
			
			var stat = hudStats[behaviour];
				stat.UpdateValue(value);
				stat.UpdateDelta(delta);
				stat.UpdateFill(percent);
		}

		void GetHUD()
		{
			var behaviours = needs.behaviours;
			foreach (BehaviourFloat need in behaviours) 
			{
				var b = need.behaviour;
				var health = needs.GetValue(b) / preset.maximumNeedsScore;
				var _mood = health.RemapNRB(0f, 1f, -1f, 1f);
				
				var mood = getMoodFromAttribute(_mood);
				var stat = Mathf.FloorToInt(Profile.GetWeight(b) * maxHudStatScore);
				var delta = Mathf.FloorToInt(profileDelta.GetValue(b) * maxHudStatScore);
				
				//UpdateHUDAttribute(b, mood);
				UpdateHUDStat(b, stat, health, delta);
			}
		}

		#endregion
	}
}