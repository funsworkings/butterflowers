using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Noder.Graphs;
using Noder.Nodes.Behaviours.Entries;
using Noder.Nodes.Behaviours.Fetch;
using Settings;
using UnityEngine;
using TMPro;

namespace Wizard {

	using Action = Actions.Action;

	using ActionType = Actions.Type;
	using Emote = Actions.Emote;

	public class Brain: MonoBehaviour {

		#region External

		[SerializeField] World Manager;


		Sun Sun;
		Library Library;

		public Nest Nest;
		public MotherOfButterflies Mother;

		#endregion

		#region Internal

		public enum MoodState { Neutral, Depression, Joyous }
		public enum Sub_MoodState { NULL, Mischief, Violence, Happy, Elation }

		public enum StanceState { Neutral, Unknown, Confidence }

		[System.Serializable]
		public struct ReactionMapping {
			public EVENTCODE @event;
			public DialogueCollection reactions;
		}

		#endregion


		#region Properties

		public BrainPreset Preset;

		public ModuleTree CoreBehaviourTree;
		public ModuleTree[] BehaviourTrees;

		[SerializeField] WorldPreset WorldPreset;

		Controller controller;
		Memories memories;
		Dialogue dialogue;
		Actions actions;
		Wand wand;

		#endregion

		#region Attributes

		[SerializeField] [Range(-1f, 1f)] float m_mood = 0f;
		[SerializeField] [Range(0f, 1f)] float m_stance = 0f;
		[SerializeField] [Range(0f, 1f)] float m_absorption = 0f;

		[SerializeField] float m_environmentKnowledge = 0f;
		[SerializeField] Knowledge[] knowledge = new Knowledge[] { };

		[SerializeField] float m_shortterm_weight = 0f;
		[SerializeField] Memory m_shortterm_memory = null;

		[SerializeField] ActionType[] m_possibleActionsFilter = new ActionType[] { };
		[SerializeField] ActionType[] m_possibleActionsRaw = new ActionType[] { };

		[SerializeField] ActionType[] excludes;

		private const int EVENT_WAIT = 36;
		private const int EVENT_STACK_HEIGHT = 12;

		#endregion

		#region Collections

		public List<Beacon> wiz_beacons = new List<Beacon>();
		public List<Beacon> dsktop_beacons = new List<Beacon>();
		public List<Beacon> all_beacons = new List<Beacon>();

		Dictionary<string, Beacon> beacon_lookup = new Dictionary<string, Beacon>();

		Dictionary<string, Knowledge> m_fileKnowledge = new Dictionary<string, Knowledge>();

		public List<EVENTCODE> PLAYER_EVENTS = new List<EVENTCODE>();
		int RECEIVE_PLAYER_EVENT = -1;
		public EVENTCODE LAST_PLAYER_EVENT => (PLAYER_EVENTS.Count > 0) ? PLAYER_EVENTS.Last() : EVENTCODE.NULL;

		public List<EVENTCODE> WORLD_EVENTS = new List<EVENTCODE>();
		int RECEIVE_WORLD_EVENT = -1;
		public EVENTCODE LAST_WORLD_EVENT => (WORLD_EVENTS.Count > 0) ? WORLD_EVENTS.Last() : EVENTCODE.NULL;

		public List<EVENTCODE> NEST_EVENTS = new List<EVENTCODE>();
		int RECEIVE_NEST_EVENT = -1;
		public EVENTCODE LAST_NEST_EVENT => (NEST_EVENTS.Count > 0) ? NEST_EVENTS.Last() : EVENTCODE.NULL;

		public ReactionMapping[] reactionMappings = new ReactionMapping[] { };

		#endregion

		#region Accessors

		public float mood => m_mood;
		public float stance => m_stance;
		public float absorption => m_absorption;

		public float environmentKnowledge => m_environmentKnowledge;
		public Dictionary<string, Knowledge> fileKnowledge => m_fileKnowledge;

		public float shortterm_weight => m_shortterm_weight;
		public Memory shortterm_memory => m_shortterm_memory;

		#endregion

		#region Threshold type definitions

		[System.Serializable] public class StanceThreshold<E> { public E type;[Range(-1f, 1f)] public float stance = 0f; }
		[System.Serializable] public class MoodThreshold<E> { public E type;[Range(-1f, 1f)] public float mood = 0f; }

		[System.Serializable] public class ActionStanceThreshold: StanceThreshold<ActionType> { }
		[System.Serializable] public class BeaconOpMoodThreshold: MoodThreshold<ActionType> { }
		[System.Serializable] public class NestOpMoodThreshold: MoodThreshold<ActionType> { }
		[System.Serializable] public class EmoteMoodThreshold: MoodThreshold<Actions.Emote> { }

		#endregion

		#region Debug

		[Header("DEBUG MENU")]

		public TMP_Text stanceUI;
		public TMP_Text moodUI;
		public TMP_Text absorbUI;

		public TMP_Text treeUI;
		public TMP_Text treeEventUI;

		public TMP_Text enviroKUI;
		public TMP_Text fileKUI;

		public TMP_Text shorttermMood;
		public TMP_Text shortTermWeight;

		public RectTransform playerActionUI_root;
		public TMP_Text[] playerActionUI;

		public RectTransform enviroActionUI_root;
		public TMP_Text[] enviroActionUI;

		public RectTransform nestActionUI_root;
		public TMP_Text[] nestActionUI;

		public RectTransform actionQueueUI_root;
		public TMP_Text currentActionUI;
		public TMP_Text[] actionQueueUI;

		public TMP_Text hobUI;
		public TMP_Text hobPlUI, hobWzUI, hobShUI, hobEvUI;
		public TMP_Text nestFillUI;

		public TMP_Text playerIntentUI;
		public TMP_Text suggestionUI;

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			controller = GetComponent<Controller>();
		}

		void OnEnable()
		{
			Manager.onRefreshBeacons += Reset;

			ModuleTree.onReceiveEvent += onReceiveEvent;
			ModuleTree.onReceiveDialogue += onReceiveDialogue;

			Events.onFireEvent += onFireEvent;
		}

		void OnDisable()
		{
			Manager.onRefreshBeacons -= Reset;

			ModuleTree.onReceiveEvent -= onReceiveEvent;
			ModuleTree.onReceiveDialogue -= onReceiveDialogue;

			Events.onFireEvent -= onFireEvent;
		}

		void Start()
		{
			Sun = Sun.Instance;
			Library = Library.Instance;
			Mother = MotherOfButterflies.Instance;

			dialogue = controller.Dialogue;
			actions = controller.Actions;
			memories = controller.Memories;
			wand = controller.Wand;

			CoreBehaviourTree = BehaviourTrees[0];
			foreach (ModuleTree tree in BehaviourTrees)
				tree.Brain = this;

			RECEIVE_PLAYER_EVENT = RECEIVE_WORLD_EVENT = RECEIVE_NEST_EVENT = -EVENT_WAIT;


			playerActionUI = playerActionUI_root.GetComponentsInChildren<TMP_Text>();
			enviroActionUI = enviroActionUI_root.GetComponentsInChildren<TMP_Text>();
			nestActionUI = nestActionUI_root.GetComponentsInChildren<TMP_Text>();

			actionQueueUI = actionQueueUI_root.GetComponentsInChildren<TMP_Text>();

			StartCoroutine("Cycle");
		}

		IEnumerator Cycle()
		{
			while (true) {
				RECEIVE_PLAYER_EVENT++;
				RECEIVE_WORLD_EVENT++;
				RECEIVE_NEST_EVENT++;


				if (!actions.inprogress) {
					if (actions.queue.Length == 0) {
						CoreBehaviourTree.Restart();
					}
				}


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


				DebugEventQueue(ref playerActionUI, ref PLAYER_EVENTS);
				DebugEventQueue(ref enviroActionUI, ref WORLD_EVENTS);
				DebugEventQueue(ref nestActionUI, ref NEST_EVENTS);
			}
		}

		void Update()
		{
			if (!Sun.active) return;

			float dt = Time.deltaTime;

			LearnFromEnvironment(dt);
			LearnFromBeacons(dt);

			var enviro = environmentKnowledge;
			var files = knowledge = fileKnowledge.Values.ToArray();
			controller.UpdateKnowledgeBase(enviro, files);

			FeelMemory(dt);

			float t_mood = EvaluateMood();
			m_mood = Mathf.Lerp(mood, t_mood, Time.deltaTime * Preset.moodSmoothSpeed);

			m_absorption = Manager.GetAbsorption();

			DebugResources();
			DebugActionQueue();
			DebugWorld();
			DebugPlayer();
		}

		void OnDestroy()
		{
			foreach (ModuleTree tree in BehaviourTrees)
				tree.Dispose();
		}

		void Reset()
		{
			Dispose();

			var beacons = Manager.AllBeacons;
			if (beacons == null || beacons.Length == 0) return;

			Refresh(beacons);

			m_stance = EvaluateStance(); // Reset stance
		}

		#endregion

		#region Operations

		public void Dispose()
		{
			all_beacons.Clear();
			wiz_beacons.Clear();
			dsktop_beacons.Clear();

			beacon_lookup.Clear();
		}

		public void Load(float enviro, Knowledge[] files)
		{
			m_environmentKnowledge = 0f;
			m_fileKnowledge = new Dictionary<string, Knowledge>();

			m_environmentKnowledge = enviro;
			if (files != null) {
				for (int i = 0; i < files.Length; i++) {
					var k = files[i];
					var file = k.file;

					if (!fileKnowledge.ContainsKey(file))
						fileKnowledge.Add(file, k);
				}
			}
		}

		#endregion




		#region Evaluate mood/stance

		float EvaluateStance()
		{
			float EW = Preset.enviroKnowledgeStanceWeight;
			float FW = Preset.fileKnowledgeStanceWeight;
			float TW = (EW + FW);

			EW /= TW;
			FW /= TW;

			float enviro = FetchKnowledgeFromEnvironment();
			float files = FetchKnowledgeFromFiles();

			return (EW * enviro) + (FW * files);
		}

		float EvaluateMood()
		{
			float SW = Preset.stanceMoodWeight;
			float BHW = Preset.healthOfButterflowersMoodWeight;
			float STMW = Preset.shortTermMemoryMoodWeight * shortterm_weight;
			float TW = (SW + BHW + STMW);

			SW /= TW;
			BHW /= TW;
			STMW /= TW;

			float stance = m_stance;
			float butterflowerhealth = Mother.GetHealth();
			float st_memory = ((shortterm_memory == null) ? 0f : shortterm_memory.weight).RemapNRB(-1f, 1f, 0f, 1f, true);

			float m = (SW * stance) + (BHW * butterflowerhealth) + (STMW * st_memory);
			m = m.RemapNRB(0f, 1f, -1f, 1f, true);

			return m;
		}

		float EvaluateAbsorption()
		{
			return Manager.GetAbsorption();
		}

		#endregion

		#region Shortterm memory effects

		// Affects mood temporarily
		public void EncounterMemory(Memory memory, bool nest = false)
		{
			if (memory == null) return;

			float startweight = (nest) ? Preset.nestShortTermMemoryEffect : Preset.defaultShortTermMemoryEffect;

			float str = Mathf.Abs(memory.weight);
			if (str >= Preset.shortTermMemoryWeightTriggerThreshold) {
				float curr_str = 0f;

				if (shortterm_memory != null) {
					curr_str = Mathf.Abs(shortterm_memory.weight);
					if (shortterm_memory == memory) {
						curr_str = 0f; // Refresh current memory
						startweight = Mathf.Max(startweight, shortterm_weight); // Choose larger memory weight
					}
				}

				if (str > curr_str) {
					m_shortterm_memory = memory;
					m_shortterm_weight = startweight;
				}
			}
		}

		void FeelMemory(float dt)
		{
			if (shortterm_memory == null) {
				m_shortterm_weight = 0f;
				return;
			}

			float decay = Preset.shortTermMemoryDecaySpeed * dt;
			m_shortterm_weight -= decay;

			if (m_shortterm_weight <= 0f) {
				m_shortterm_weight = 0f;
				m_shortterm_memory = null;
			}
		}

		#endregion

		#region Learning

		void LearnFromEnvironment(float dt)
		{
			m_environmentKnowledge += dt;
		}

		void LearnFromBeacons(float dt)
		{
			if (dsktop_beacons == null) return;

			for (int i = 0; i < dsktop_beacons.Count; i++) {
				var beacon = dsktop_beacons[i];
				var accel = Nest.HasBeacon(beacon);

				float multiplier = (accel) ? Preset.nestLearningMultiplier : Preset.defaultLearningMultiplier;
				Learn(beacon, dt, multiplier);
			}
		}

		void Learn(Beacon beacon, float dt, float multiplier = 1f)
		{
			if (beacon == null) return;

			var file = beacon.file;
			if (!fileKnowledge.ContainsKey(file)) return;

			var k = fileKnowledge[file];
			k.AddTime(dt * multiplier);
		}

		void Refresh(Beacon[] beacons)
		{
			Dispose();

			for (int i = 0; i < beacons.Length; i++) {
				var beacon = beacons[i];
				if (beacon != null) {
					AddKnowledge(beacon.file);

					if (Library.IsDesktop(beacon)) dsktop_beacons.Add(beacon);
					else if (Library.IsWizard(beacon) || Library.IsShared(beacon)) wiz_beacons.Add(beacon);

					var file = beacon.file;
					if (!beacon_lookup.ContainsKey(file))
						beacon_lookup.Add(file, beacon);
				}
			}

			all_beacons = new List<Beacon>(beacons);
		}

		void AddKnowledge(string file)
		{
			if (fileKnowledge.ContainsKey(file)) return;

			var k = new Knowledge();
			k.file = file;
			k.time = 0f;

			fileKnowledge.Add(file, k);
		}

		#endregion

		#region Actions

		public void ParseBeaconEvent(EVENTCODE @event, object dat) 
		{
			var file = (string)dat;
			Beacon beacon = null;

			beacon_lookup.TryGetValue(file, out beacon);

			if (beacon != null) {
				bool immediate = !EnsureNestActive();

				if(@event == EVENTCODE.BEACONACTIVATE)
					actions.Push(ActionType.BeaconActivate, beacon);
				if (@event == EVENTCODE.NESTPOP)
					actions.Push(ActionType.NestPop, beacon);
			}
		}

		public void ParseNestEvent(EVENTCODE @event, object dat) 
		{
			if (@event == EVENTCODE.NESTPOP)
				ParseBeaconEvent(@event, dat);
			if (@event == EVENTCODE.NESTCLEAR) 
			{
				bool immediate = !EnsureNestActive();
				actions.Push(ActionType.NestClear, null);
			}
			if (@event == EVENTCODE.NESTKICK) 
			{
				Wand.Kick kick = (Wand.Kick)dat;
				actions.Push(ActionType.NestKick, kick);
			}
		}

		public void ParseMiscellaneousEvent(EVENTCODE @event, object dat) 
		{
			if (@event == EVENTCODE.GESTURE) {
				Gesture gesture = (Gesture)dat;
				actions.Push(ActionType.Gesture, gesture);
			}
			if (@event == EVENTCODE.PHOTOGRAPH) {
				string name = (string)dat;
				actions.Push(ActionType.Picture, name);
			}
		}

		public void ReactToEvent()
		{
			var rand = Random.Range(0f, 1f);
			if (rand >= Preset.reactionProbability)
				return;

			EVENTCODE @event = Events.LAST_EVENT;

			var mappings = reactionMappings.Where(m => m.@event == @event);
			if (mappings.Count() == 0) return;

			var map = mappings.ElementAt(0); // Get first mapping
			var reaction = map.reactions.elements.PickRandomSubset(1)[0];

			dialogue.Push(reaction, true);
		}

		#endregion

		#region Events

		void onFireEvent(EVENTCODE @event, AGENT a, AGENT b, string detail)
		{
			string event_name = System.Enum.GetName(typeof(EVENTCODE), @event);
			if (event_name.Contains("NEST")) {
				AddEventToQueue(@event, ref NEST_EVENTS);
				RECEIVE_NEST_EVENT = -EVENT_WAIT;
			}
			if (a == AGENT.Inhabitant0 || a == AGENT.Inhabitants) {
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


		#region Behaviour tree (CALLBACKS)

		void onReceiveEvent(ModuleTree tree, EVENTCODE @event, object data)
		{
			bool absorb = (tree.name == "AUTO ACTION MODULE");
			controller.SetAbsorbState(absorb);


			string type = System.Enum.GetName(typeof(EVENTCODE), @event);
			if (type.Contains("BEACON")) 
			{
				ParseBeaconEvent(@event, data);
			}
			else if (type.Contains("NEST")) 
			{
				ParseNestEvent(@event, data);
			}
			else
			{
				ParseMiscellaneousEvent(@event, data);
			}

			treeUI.text = tree.name;
			treeEventUI.text = System.Enum.GetName(typeof(EVENTCODE), @event);
		}

		void onReceiveDialogue(ModuleTree tree, string dialogue, float delay)
		{
			actions.Push(ActionType.Dialogue, dialogue, delay:delay);
		}

		#endregion

		#region Behaviour tree (FETCH)

		public MoodState getMoodState()
		{
			if (mood >= Preset.highMoodThreshold)
				return MoodState.Joyous;
			if (mood <= Preset.lowMoodThreshold)
				return MoodState.Depression;

			return MoodState.Neutral;
		}

		public Sub_MoodState getSubMoodState()
		{
			var mood = this.mood;
			var state = getMoodState();

			if (state == MoodState.Depression) 
			{
				var thresh = Preset.lowMoodThreshold;
				var max = Mathf.Abs(-1f - mood);
				var diff = Mathf.Abs(mood - thresh);

				if (diff / max >= .5f)
					return Sub_MoodState.Violence;
				else
					return Sub_MoodState.Mischief;
			}
			else if (state == MoodState.Joyous) 
			{
				var thresh = Preset.highMoodThreshold;
				var max = Mathf.Abs(1f - mood);
				var diff = Mathf.Abs(mood - thresh);

				if (diff / max >= .5f)
					return Sub_MoodState.Elation;
				else
					return Sub_MoodState.Happy;
			}

			return Sub_MoodState.NULL;
		}


		public StanceState getStanceState(AGENT agent = AGENT.NULL)
		{
			float stance = this.stance;

			if (agent == AGENT.World)
				stance = FetchKnowledgeFromEnvironment(); // Pull down enviro knowledge instead


			if (stance >= Preset.highStanceThreshold)
				return StanceState.Confidence;
			if (stance <= Preset.lowStanceThreshold)
				return StanceState.Unknown;

			return StanceState.Neutral;
		}

		public string[] getBeacons() { return Manager.AllBeacons.Select(beacon => beacon.file).ToArray(); }



		public SUGGESTION getSuggestion()
		{
			var possible = Manager.GetSuggestions();
			return possible.PickRandomSubset(1)[0];
		}


		// INITIAL IMPL FOR INTENT -- todo (better one)
		public INTENT getPlayerIntent()
		{
			float action_impact = 0f;

			int total = 0;
			foreach (EVENTCODE e in PLAYER_EVENTS) 
			{
				action_impact += EVENT_WEIGHT_LOOKUP.WEIGHTS[e];
				++total;
			}
			action_impact /= total;

			float mood_impact = mood;
			float hob_impact = (1f - Mother.player_hatred).RemapNRB(0f, 1f, -1f, 1f); // Invert HoB hatred => more hatred, more negative impact

			float impact_total = Preset.impactActionWeight + Preset.impactMoodWeight + Preset.impactHoBWeight;

			float ia = Preset.impactActionWeight / impact_total;
			float im = Preset.impactMoodWeight / impact_total;
			float ihob = Preset.impactHoBWeight / impact_total;

			float impact = action_impact*ia + mood_impact*im + hob_impact*ihob;

			if (impact < -.33f)
				return INTENT.FOIL;
			if (impact > .33f)
				return INTENT.PLAY;
			if (Mathf.Abs(impact) < .167f)
				return INTENT.OBSERVE;

			return INTENT.UNKNOWN;
		}

		#endregion

		#region Behaviour tree (OPS)

		public string[] filterMemoryBeacons(string[] raw, int mood)
		{
			List<string> filtered = new List<string>();

			var intersect  = raw.Intersect(wiz_beacons.Select(beacon => beacon.file));
			float test_sign = Mathf.Sign(mood);

			foreach (string beacon in intersect) {
				var mem = memories.GetMemoryByName(beacon);

				if (mem != null) {
					if(Mathf.Sign(mem.weight) == mood)
						filtered.Add(beacon);
				}
			}

			return filtered.ToArray();
		}

		public string[] filterBeacons(string[] raw, FilterBeacons.Filter filter)
		{
			var ek = FetchKnowledgeFromEnvironment();

			List<Beacon> source = new List<Beacon>();
			foreach (string key in raw) {
				var beacon = beacon_lookup[key];
				if (beacon != null)
					source.Add(beacon);
			}

			switch (filter) 
			{
				case FilterBeacons.Filter.Active:
					source = source.Where(b => !b.visible).ToList();
					break;
				case FilterBeacons.Filter.Inactive:
					source = source.Where(b => b.visible).ToList();
					break;
				case FilterBeacons.Filter.Unknown:
					source = source.Where(b => FetchKnowledgeFromBeacon(b) <= .5f).ToList();
					break;
				case FilterBeacons.Filter.Comfortable:
					source = source.Where(b => FetchKnowledgeFromBeacon(b) > .5f).ToList();
					break;
				case FilterBeacons.Filter.Memory:
					source = source.Intersect(wiz_beacons).ToList();
					break;
				case FilterBeacons.Filter.Playable:
				case FilterBeacons.Filter.Actionable:
				default:
					break;
			}

			return source.Select(beacon => beacon.file).ToArray();
		}

		public void Burst() { Manager.PositiveBurst(); }

		#endregion

		#region Behaviour tree (HELPERS)

		bool EnsureNestActive()
		{
			if (!Nest.open) 
			{
				Wand.Kick kick = new Wand.Kick();
				kick.useDirection = false;

				actions.Push(ActionType.NestKick, kick);
				return true;
			}

			return false;
		}

		#endregion


		#region Knowledge accessors

		float FetchKnowledgeFromEnvironment()
		{
			var e = environmentKnowledge;
			float days_e = WorldPreset.ConvertToDays(e);

			return Mathf.Clamp01(days_e / Preset.daysUntilEnvironmentKnowledge);
		}

		float FetchKnowledgeFromFiles() 
		{
			float files = 0f;

			int files_len = all_beacons.Count;
			for (int i = 0; i < files_len; i++) {
				var beacon = all_beacons[i];
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
			if (string.IsNullOrEmpty(file) || !fileKnowledge.ContainsKey(file)) return 0f;
			if (Library.IsShared(file) || Library.IsWizard(file)) return 1f;

			var k = fileKnowledge[file];
			float days_k = WorldPreset.ConvertToDays(k.time);

			return Mathf.Clamp01(days_k / Preset.daysUntilFileKnowledge);
		}

		#endregion

		#region Possible action checks

		public ActionType[] GetPossibleBeaconOps()
		{
			return Preset.beaconOpMoodThresholds.Where(beaconOp => MatchesMood(beaconOp.mood)).Select(beaconOp => beaconOp.type).ToArray(); 
		}

		public ActionType[] GetPossibleNestOps()
		{
			return Preset.nestOpMoodThresholds.Where(nestOp => MatchesMood(nestOp.mood)).Select(nestOp => nestOp.type).ToArray();
		}

		public Gesture[] GetPossibleGestures()
		{
			return new Gesture[] { };
		}

		public Emote[] GetPossibleEmotes()
		{
			return Preset.emoteMoodThresholds.Where(emote => MatchesMood(emote.mood)).Select(emote => emote.type).ToArray();
		}


		#endregion

		#region Actionable item checks

		public bool ExistsActionableBeaconOp()
		{
			return Preset.beaconOpMoodThresholds.Any(beaconOp => MatchesMood(beaconOp.mood));
		}

		public bool ExistsActionableBeacon(ActionType op)
		{
			if (op == ActionType.BeaconActivate)
				return Manager.InactiveBeacons.Where(beacon => FetchKnowledgeFromBeacon(beacon) >= Preset.minimumBeaconActivateKnowledge).Count() > 0;
			if (op == ActionType.BeaconDestroy)
				return Manager.InactiveBeacons.Where(beacon => FetchKnowledgeFromBeacon(beacon) <= Preset.maximumBeaconDeleteKnowledge).Count() > 0;

			return false;
		}

		public bool ExistsActionableNestOp()
		{
			return Preset.nestOpMoodThresholds.Any(nestOp => MatchesMood(nestOp.mood));
		}

		public bool ExistsActionableGesture()
		{
			return false;
		}

		public bool ExistsActionableEmote()
		{
			return Preset.emoteMoodThresholds.Any(emote => MatchesMood(emote.mood));
		}

		public bool MatchesMood(float weight)
		{
			if (Mathf.Abs(weight) < .1f) return true;

			if (mood > 0f) return (weight > 0f);
			if (mood < 0f) return (weight < 0f);

			return true;
		}

		#endregion



		#region Debug

		void DebugResources()
		{
			var s_state = getStanceState();
			var m_state = getMoodState();

			// DEBUG
			stanceUI.text = string.Format("{0} ({1})", stance, System.Enum.GetName(typeof(StanceState), s_state));
			moodUI.text = string.Format("{0} ({1})", mood, System.Enum.GetName(typeof(MoodState), m_state));
			absorbUI.text = string.Format("{0}%", Mathf.RoundToInt(absorption * 100f));

			enviroKUI.text = FetchKnowledgeFromEnvironment() + "";
			fileKUI.text = FetchKnowledgeFromFiles() + "";

			shorttermMood.text = (shortterm_memory == null)? "NULL":shortterm_memory.weight+"";
			shortTermWeight.text = (shortterm_memory == null)? "0.0":m_shortterm_weight+"";
		}

		void DebugEventQueue(ref TMP_Text[] arr, ref List<EVENTCODE> events)
		{
			for (int i = 0; i < arr.Length; i++) 
			{
				var valid = i < events.Count;

				arr[i].enabled = valid;
				if (valid)
					arr[i].text = System.Enum.GetName(typeof(EVENTCODE), events[i]);
			}
		}

		void DebugActionQueue()
		{
			var queue = actions.queue;

			for (int i = 0; i < actionQueueUI.Length; i++) {
				var valid = i < queue.Length;

				actionQueueUI[i].enabled = valid;
				if (valid) {
					var action = queue[i];
					actionQueueUI[i].text = System.Enum.GetName(typeof(Actions.Type), action.type);
				}
			}

			if (actions.currentAction == null)
				currentActionUI.text = "NULL";
			else
				currentActionUI.text = System.Enum.GetName(typeof(Actions.Type), actions.currentAction.type);
		}

		void DebugWorld()
		{
			hobUI.text = string.Format("{0}%", Mathf.RoundToInt(Mother.GetHealth() * 100f));
			hobPlUI.text = string.Format("{0}%", Mathf.RoundToInt(Mother.player_hatred * 100f));
			hobWzUI.text = string.Format("{0}%", Mathf.RoundToInt(Mother.wiz_hatred * 100f));
			hobShUI.text = string.Format("{0}%", Mathf.RoundToInt(Mother.shared_hatred * 100f));
			hobEvUI.text = string.Format("{0}%", Mathf.RoundToInt(Mother.enviro_hated * 100f));

			nestFillUI.text = string.Format("{0}%", Mathf.RoundToInt(Nest.fill * 100f));
		}

		void DebugPlayer()
		{
			var intent = getPlayerIntent();
			playerIntentUI.text = System.Enum.GetName(typeof(INTENT), intent);

			var suggestion = getSuggestion();
			suggestionUI.text = System.Enum.GetName(typeof(SUGGESTION), suggestion);
		}

		#endregion




		#region Deprecated

		/*
		 * 
		 * 
		 * 		public ActionType DecideActionType()
		{
			var possible = GetPossibleActions();
			if (possible.Length == 0) return ActionType.None;

			var el = possible.PickRandomSubset(1)[0];
			return el;
		}

		public Action ChooseBestAction()
		{
			ActionType[] available = m_possibleActionsFilter = GetPossibleActions();
			if (available.Length == 0) return null;

			var action = new Action();
			action.type = ActionType.None;

			ActionType choice = available.PickRandomSubset(1)[0];
			string name = System.Enum.GetName(typeof(ActionType), choice);

			if (name.StartsWith("Beacon")) {
				var ops = GetPossibleBeaconOps();
				if (ops.Length > 0) {
					var op = ops.PickRandomSubset(1)[0];

					var beacon = GetActionableBeaconForBeaconOp(op);
					bool success = (beacon != null);

					if (success) {
						action.type = op;
						action.dat = beacon;
					}
				}
			}
			if (name.StartsWith("Nest")) {
				var ops = GetPossibleNestOps();
				if (ops.Length > 0) {
					var op = ops.PickRandomSubset(1)[0];
					Beacon bdat = null;

					bool success = true;
					if (op != ActionType.NestKick) {
						var beacons = Manager.ActiveBeacons;

						if (beacons.Length == 0)
							success = false;
						else {
							if (op == ActionType.NestPop) 
							{
								var beacon = beacons.PickRandomSubset(1)[0];
								bdat = beacon;
							}
						}
					}

					if (success) {
						action.type = op;
						action.dat = bdat;
					}
				}
			}
			if (choice == ActionType.Gesture) {
				var gestures = GetPossibleGestures();
				if (gestures.Length > 0) {
					var gesture = gestures.PickRandomSubset(1)[0];

					action.type = ActionType.Gesture;
					action.dat = gesture;
				}
			}
			if (choice == ActionType.Picture) {
				action.type = ActionType.Picture;
			}
			if (choice == ActionType.Dialogue) {
				action.type = ActionType.Dialogue;
				action.dat = null;
			}
			if (choice == ActionType.Emote) {
				var emotes = GetPossibleEmotes();
				if (emotes.Length > 0) {
					var emote = emotes.PickRandomSubset(1)[0];

					action.type = ActionType.Emote;
					action.dat = emote;
				}
			}

			return action;
		}

		public ActionType[] GetPossibleActions()
		{
			IEnumerable<ActionType> raw = Preset.actionStanceThresholds.Where(typeThreshold => stance >= typeThreshold.stance).Select(typeThreshold => typeThreshold.type);
			List<ActionType> filtered = new List<ActionType>();

			m_possibleActionsRaw = raw.ToArray();

			for (int i = 0; i < raw.Count(); i++) {
				var type = raw.ElementAt(i);
				var name = System.Enum.GetName(typeof(ActionType), type);

				if (excludes.Contains(type))
					continue;


				if (name.StartsWith("Beacon")) {
					if (!ExistsActionableBeaconOp())
						continue;
				}
				if (name.StartsWith("Nest")) {
					if (!ExistsActionableNestOp())
						continue;
				}
				if (type == ActionType.Gesture) {
					if (!ExistsActionableGesture())
						continue;
				}
				if (type == ActionType.Emote) {
					if (!ExistsActionableEmote())
						continue;
				}

				filtered.Add(type);
			}

			return filtered.ToArray();
		}

		
		public Beacon GetActionableBeaconForBeaconOp(ActionType op)
		{
			var beacons = Manager.InactiveBeacons;

			Beacon _beacon = null;

			if (op == ActionType.BeaconActivate) {
				if (Nest.open) 
				{
					var possible = beacons.Where(beacon => FetchKnowledgeFromBeacon(beacon) >= Preset.minimumBeaconActivateKnowledge).ToArray();
					if (possible.Length > 0)
						_beacon = possible.PickRandomSubset(1)[0];
				}
			}
			else if (op == ActionType.BeaconDestroy) 
			{
				var possible = beacons.Where(beacon => FetchKnowledgeFromBeacon(beacon) <= Preset.maximumBeaconDeleteKnowledge).ToArray();
				if (possible.Length > 0)
					_beacon = possible.PickRandomSubset(1)[0];
			}

			return _beacon;
		}
		 * 
		 * 
		 * 
		 */

		#endregion
	}

}