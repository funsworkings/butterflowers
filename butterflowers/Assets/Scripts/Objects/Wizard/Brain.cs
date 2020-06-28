using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Settings;
using UnityEngine;

namespace Wizard {

	using Action = Actions.Action;

	using ActionType = Actions.Type;
	using Gesture = Wand.Gesture;
	using Emote = Actions.Emote;

	public class Brain: MonoBehaviour {

		#region External

		[SerializeField] Manager Manager;
		[SerializeField] Nest Nest;

		Sun Sun;
		Library Library;
		MotherOfButterflies Mother;

		#endregion

		#region Properties

		[SerializeField] BrainPreset Preset;
		[SerializeField] WorldPreset WorldPreset;

		Controller controller;
		Memories memories;
		Dialogue dialogue;
		Actions actions;
		Wand wand;

		#endregion

		#region Attributes

		[SerializeField] float m_mood = 0f;
		[SerializeField] float m_stance = 0f;

		[SerializeField] float m_environmentKnowledge = 0f;
		[SerializeField] Knowledge[] knowledge = new Knowledge[] { };

		[SerializeField] float m_shortterm_weight = 0f;
		[SerializeField] Memory m_shortterm_memory = null;

		[SerializeField] ActionType[] m_possibleActionsFilter = new ActionType[] { };
		[SerializeField] ActionType[] m_possibleActionsRaw = new ActionType[] { };

		[SerializeField] ActionType[] excludes;

		#endregion

		#region Collections

		List<Beacon> wiz_beacons = new List<Beacon>();
		List<Beacon> dsktop_beacons = new List<Beacon>();
		List<Beacon> all_beacons = new List<Beacon>();

		Dictionary<string, Knowledge> m_fileKnowledge = new Dictionary<string, Knowledge>();

		#endregion

		#region Internal

		[System.Serializable]
		public struct ActionTypeThreshold {
			public ActionType type;

			[Range(0f, 1f)]
			public float threshold;
		}

		[System.Serializable]
		public struct BeaconOpWeight {
			public ActionType type;

			[Range(-1f, 1f)]
			public float weight;
		}

		[System.Serializable]
		public struct NestOpWeight {
			public ActionType type;

			[Range(-1f, 1f)]
			public float weight;
		}

		[System.Serializable]
		public struct EmoteWeight {
			public Actions.Emote emote;

			[Range(-1f, 1f)]
			public float weight;
		}

		#endregion

		#region Accessors

		public float mood => m_mood;
		public float stance => m_stance;

		public float environmentKnowledge => m_environmentKnowledge;
		public Dictionary<string, Knowledge> fileKnowledge => m_fileKnowledge;

		public float shortterm_weight => m_shortterm_weight;
		public Memory shortterm_memory => m_shortterm_memory;

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			controller = GetComponent<Controller>();
		}

		void OnEnable()
		{
			Manager.onRefreshBeacons += Reset;
		}

		void OnDisable()
		{
			Manager.onRefreshBeacons -= Reset;
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
		}

		void Reset()
		{
			Dispose();

			var beacons = Manager.AllBeacons;
			if (beacons == null || beacons.Length == 0) return;

			Refresh(beacons);

			m_stance = EvaluateStance();
		}

		#endregion

		#region Internal

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
				Learn(beacon, dt, multiplier); //todo: accelerate beacon learning when inside nest
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

		float EvaluateStance()
		{
			float EW = Preset.environmentWeight;
			float FW = Preset.filesWeight;
			float TW = (EW + FW);

			EW /= TW;
			FW /= TW;

			float enviro = FetchKnowledgeFromEnvironment();
			float files = 0f;

			int files_len = all_beacons.Count;
			for (int i = 0; i < files_len; i++) {
				var beacon = all_beacons[i];
				float k = FetchKnowledgeFromBeacon(beacon);

				files += k;
			}
			files /= ((float)files_len);

			Debug.LogFormat("enviro = {0} files = {1}", enviro, files);

			return (EW * enviro) + (FW * files);
		}

		float EvaluateMood()
		{
			float SW = Preset.stanceWeight;
			float BHW = Preset.butterflyHealthWeight;
			float STMW = Preset.shortTermMemoryWeight * shortterm_weight;
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

		#endregion

		#region Helpers

		float FetchKnowledgeFromEnvironment()
		{
			var e = environmentKnowledge;
			float days_e = WorldPreset.ConvertToDays(e);

			return Mathf.Clamp01(days_e / Preset.daysUntilEnvironmentKnowledge);
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

		#region Learning

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

		#region Operations

		public void Dispose()
		{
			all_beacons.Clear();
			wiz_beacons.Clear();
			dsktop_beacons.Clear();
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

		public ActionType DecideActionType()
		{
			var possible = GetPossibleActions();
			if (possible.Length == 0) return ActionType.None;

			var el = possible.PickRandomSubset(1)[0];
			return el;
		}

		#endregion

		#region Actions

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
			IEnumerable<ActionType> raw = Preset.actionTypeThresholdLookup.Where(typeThreshold => stance >= typeThreshold.threshold).Select(typeThreshold => typeThreshold.type);
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

		public ActionType[] GetPossibleBeaconOps()
		{
			return Preset.beaconOpWeightLookup.Where(beaconOp => MatchesMood(beaconOp.weight)).Select(beaconOp => beaconOp.type).ToArray();
		}

		public ActionType[] GetPossibleNestOps()
		{
			return Preset.nestOpWeightLookup.Where(nestOp => MatchesMood(nestOp.weight)).Select(nestOp => nestOp.type).ToArray();
		}

		public Gesture[] GetPossibleGestures()
		{
			if (!Nest.Instance.open) return new Gesture[] { };
			return wand.gestures.Where(gesture => MatchesMood(gesture.weight)).ToArray();
		}

		public Emote[] GetPossibleEmotes()
		{
			return Preset.emoteWeightLookup.Where(emote => MatchesMood(emote.weight)).Select(emote => emote.emote).ToArray();
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

		#endregion

		#region Helpers

		public bool ExistsActionableBeaconOp()
		{
			return Preset.beaconOpWeightLookup.Any(beaconOp => MatchesMood(beaconOp.weight));
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
			return Preset.nestOpWeightLookup.Any(nestOp => MatchesMood(nestOp.weight));
		}

		public bool ExistsActionableGesture()
		{
			return wand.gestures.Any(gesture => MatchesMood(gesture.weight));
		}

		public bool ExistsActionableEmote()
		{
			return Preset.emoteWeightLookup.Any(emote => MatchesMood(emote.weight));
		}

		public bool MatchesMood(float weight)
		{
			if (Mathf.Abs(weight) < .1f) return true;

			if (mood > 0f) return (weight > 0f);
			if (mood < 0f) return (weight < 0f);

			return true;
		}

		#endregion
	}

}