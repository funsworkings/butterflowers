using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Settings;
using UnityEngine;

namespace Wizard {

	public class Brain: MonoBehaviour {

		#region External

		[SerializeField] Manager Manager;
		[SerializeField] Nest Nest;

		Sun Sun;
		Library Library;

		#endregion

		#region Properties

		[SerializeField] BrainPreset Preset;
		[SerializeField] WorldPreset WorldPreset;

		Controller controller;
		Memories memories;
		Dialogue dialogue;

		#endregion

		#region Attributes

		[SerializeField] float m_mood = 0f;
		[SerializeField] float m_stance = 0f;

		[SerializeField] float m_environmentKnowledge = 0f;
		[SerializeField] Knowledge[] knowledge = new Knowledge[] { };

		#endregion

		#region Collections

		List<Beacon> wiz_beacons = new List<Beacon>();
		List<Beacon> dsktop_beacons = new List<Beacon>();
		List<Beacon> all_beacons = new List<Beacon>();

		Dictionary<string, Knowledge> m_fileKnowledge = new Dictionary<string, Knowledge>();

		#endregion

		#region Accessors

		public float mood => m_mood;
		public float stance => m_stance;

		public float environmentKnowledge => m_environmentKnowledge;
		public Dictionary<string, Knowledge> fileKnowledge => m_fileKnowledge;

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

			dialogue = controller.Dialogue;
			memories = controller.Memories;
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
		}

		void Reset()
		{
			Dispose();

			var beacons = Manager.AllBeacons;
			if (beacons == null || beacons.Length == 0) return;

			Refresh(beacons);

			m_mood = EvaluateMood();
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

				float multiplier = (accel) ? Preset.nestLearningMultiplier : 1f;
				Learn(beacon, dt, multiplier); //todo: accelerate beacon learning when inside nest
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
			for (int i = 0; i < files_len; i++) 
			{
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
			return 0f;
		}

		#endregion

		#region Helpers

		float FetchKnowledgeFromEnvironment()
		{
			var e = environmentKnowledge;
			float days_e = WorldPreset.ConvertToDays(e);

			return Mathf.Clamp01(days_e / Preset.daysUntilEnvironmentKnowledge);
		}

		float FetchKnowledgeFromBeacon(Beacon beacon)
		{
			var file = beacon.file;
			
			if (!fileKnowledge.ContainsKey(file)) return 0f;
			if (wiz_beacons.Contains(beacon)) return 1f; // Override knowledge for memories

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
				if (beacon != null) 
				{
					AddKnowledge(beacon.file);

					if (Library.IsDesktop(beacon)) dsktop_beacons.Add(beacon);
					else if (Library.IsWizard(beacon)) wiz_beacons.Add(beacon);
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
		public void EncounterMemory(Memory memory)
		{

		}

		#endregion
	}

}