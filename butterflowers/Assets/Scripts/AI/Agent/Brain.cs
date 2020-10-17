using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI.Types;
using AI.Types.Mappings;
using Settings;
using UnityEngine;
using uwu;
using Action = AI.Types.Action;
using Behaviour = AI.Types.Behaviour;

namespace AI.Agent
{
	public class Brain : MonoBehaviour
	{
		// External

		GameDataSaveSystem Save;
		Library Library;
		Sun Sun;
		World World;

		[SerializeField] BeaconManager Beacons;
		[SerializeField] Nest Nest;
		[SerializeField] Wand Wand;
		
		// Properties

		[SerializeField] WorldPreset worldPreset;
		[SerializeField] BehaviourProfile m_profile;
		[SerializeField] Scriptables.BrainPreset preset;

		Actions Actions;
		
		// Collections

		[SerializeField] BehaviourInt health;
		[SerializeField] Behaviour[] m_needs = null;

		[SerializeField] float m_enviroKnowledge = 0f;
		[SerializeField] Knowledge[] m_fileKnowledge = null;
			Dictionary<string, Knowledge> fileKnowledgeLookup = new Dictionary<string, Knowledge>();
		
		// Attributes

		[SerializeField] bool load = false, active = false;
		[SerializeField] float m_decaySpeed = -1f;
		
		#region Accessors

		public bool isActive => active;
		public bool Self => World.Parallel;

		public BehaviourProfile Profile
		{
			get { return m_profile; }
			set { m_profile = value;  }
		}

		public Behaviour[] needs
		{
			get
			{
				m_needs = Profile.weight.behaviours;
				return m_needs;
			}
		}

		public float environmentKnowledge => m_enviroKnowledge;

		public Knowledge[] fileKnowledge
		{
			get { return m_fileKnowledge;  }
			set { m_fileKnowledge = value; }
		}

		public float decaySpeed
		{
			get
			{
				if (m_decaySpeed < 0f)
					m_decaySpeed = Profile.baseDecaySpeed;

				return m_decaySpeed;
			}
		}
		
		#endregion
		
		#region Monobehaviours

		void Awake()
		{
			Actions = GetComponent<Actions>();
		}

		void OnEnable()
		{
			Advertiser.onCollectReward += OnReceiveReward;

			StartCoroutine("ActionLookup");
		}

		void OnDisable()
		{
			Advertiser.onCollectReward -= OnReceiveReward;
			
			StopAllCoroutines();
		}

		void Start()
		{
			World = World.Instance;
		}
		
		void Update()
		{
			if (!load || !Sun.active)
				return;
			
			active = World.state != World.State.User;

			KnowledgeUpdate();
			ActionUpdate();
		}

		void OnDestroy()
		{
			Dispose();
		}

		IEnumerator ActionLookup()
		{
			while (true) 
			{
				if (!Actions.available && active) // Wait till wand is available
				{
					var action = CalculateBestAction();
					if (action != null)
						Actions.Push(action); // Add action to queue

					Debug.LogFormat("Action decided => {0}", (action == null) ? "NULL" : action.@event.ToString());
				}

				yield return new WaitForSeconds(1.3f);
			}
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
			
			load = true;
		}
		
		#endregion
		
		#region Core updates

		void KnowledgeUpdate()
		{
			float dt = Time.deltaTime;
			
			LearnFromEnvironment(dt);
			LearnFromBeacons(dt);
			
			fileKnowledge = ScrapeKnowledgeLookup();

			// Update save file
			Save.enviro_knowledge = environmentKnowledge;
			Save.file_knowledge = fileKnowledge;
		}

		void ActionUpdate()
		{
			if(active) 
				DecayAllNeeds();
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
		
		#region Decay

		void DecayAllNeeds()
		{
			foreach (Behaviour behaviour in needs) {
				var value = preset.maximumNeedsScore;
				
				try {
					value = health.GetValue(behaviour);
					health.SetValue(behaviour, Mathf.Max(0, Mathf.FloorToInt(value - 1f * Time.deltaTime)));
				}
				catch (System.Exception err) 
				{
					health.SetValue(behaviour, value);
				}
				
				//var decay = Profile.decay.GetValue(behaviour);
			}
		}
		
		#endregion
		
		#region Scoring

		Action CalculateBestAction()
		{
			var advertisers = FindObjectsOfType<Advertiser>();

			Action bestAction = null;
			var bestScore = 0f;

			for (int i = 0; i < advertisers.Length; i++) 
			{
				var advertiser = advertisers[i];
				var ads = advertiser.Advertisements;

				for (int j = 0; j < ads.Length; j++) {
					var ad = ads[j];
					var rewards = ad.rewards.mappings;

					foreach (BehaviourIntegerMap map in rewards) {
						var sc = CalculateScoreForReward(map.behaviour, map.value);
						if (sc > bestScore) {
							bestScore = sc;
							bestAction = ad.action;
						}
					}
				}				
			}

			return bestAction;
		}

		float CalculateScoreForReward(Behaviour need, int reward)
		{
			int max = preset.maximumNeedsScore;
			
			int current = health.ContainsBehaviour(need)? health.GetValue(need) : preset.maximumNeedsScore;
			int future = Mathf.Clamp(current + reward, 0, max);

			float currentAtten = preset.needAttenuationCurve.Evaluate((float)current / max);
			float futureAtten = preset.needAttenuationCurve.Evaluate((float) future / max);

			return currentAtten - futureAtten;
		}
		
		#endregion
		
		#region Rewards

		void OnReceiveReward(Advertisement advertisement, Behaviour need, int reward)
		{
			try {
				var currentHealth = health.GetValue(need);
				health.SetValue(need, currentHealth + reward); // Add reward to health of need
			}
			catch (System.Exception err) {
				health.SetValue(need, reward);
			}
		}
		
		#endregion
		
		#region Disposal

		void Dispose()
		{
			fileKnowledgeLookup.Clear();
			Profile.Dispose();
		}
		
		#endregion
	}
}