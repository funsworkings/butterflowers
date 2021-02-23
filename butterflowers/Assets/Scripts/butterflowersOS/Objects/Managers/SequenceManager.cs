using System.Collections;
using System.Linq;
using butterflowersOS.Core;
using butterflowersOS.Data;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Miscellaneous;
using butterflowersOS.Presets;
using Neue.Reference.Types;
using Neue.Reference.Types.Maps;
using TMPro;
using UnityEngine;
using uwu;
using uwu.Dialogue;
using uwu.Extensions;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Objects.Managers
{
	public class SequenceManager : MonoBehaviour, ISaveable, IPauseSun
	{
		#region Internal

		public enum TriggerReason
		{
			Success,
			Block,
			
			CageNotCompleted,
			SequenceHasCompleted
		}
		
		#endregion
		
		
		// External

		Sun Sun;
		GameDataSaveSystem _Save;
		
		[SerializeField] Cage Cage = null;
		[SerializeField] Focusing Focus;
		[SerializeField] ButterflowerManager Butterflowers;
		[SerializeField] CutsceneManager Cutscenes = null;

		[SerializeField] Transform root = null;
		
		// Properties

		[SerializeField] WorldPreset preset = null;
		[SerializeField] ToggleOpacity opacity, frameOpacity;
		[SerializeField] TMP_Text frameText;
		[SerializeField] DialogueHandler sceneCaption = null;
		[SerializeField] AudioSource sceneAudio;

		Sequence[] sequences;

		[SerializeField] int index = -1;
		[SerializeField] Frame[] frames = new Frame[]{};
		[SerializeField] bool inprogress = false;

		#region Accessors

		public bool Pause => inprogress;
		public bool Complete => (Cage.Completed && (index + 1) < frames.Length);
		
		#endregion
		
		
		void Start()
		{
			Sun = Sun.Instance;
			_Save = GameDataSaveSystem.Instance;

			frames = new Frame[7];
			sequences = root.GetComponentsInChildren<Sequence>();
			
			sceneCaption.Dispose(); // Clear text in caption
		}

		#region Save/load
		
		public object Save()
		{
			SequenceData seq = new SequenceData();
			seq.index = (sbyte)index;
			seq.frames = frames.Select(fr => (byte)fr).ToArray();
			
			return seq;
		}

		public void Load(object data)
		{
			SequenceData seq = (SequenceData) data;
			if(!preset.persistSequence) 
				seq = new SequenceData();
			
			
			index = seq.index;
			frames = seq.frames.Select(fr => (Frame)fr).ToArray();

			foreach (Sequence sequence in sequences) 
			{
				sequence.Dispose();
			}

			if (index >= 0) 
			{
				for (int i = 0; i <= index; i++) // Reload all previous scenes
				{
					Frame frame = frames[i]; Debug.LogWarningFormat("Find sequence ({0}) for {1}", i, frame);
					Sequence sequence = FetchSequence(frame);
					
					if (sequence != null) 
					{
						sequence.Trigger(i, load:true);
					}
				}
			}
		}
		
		#endregion

		public TriggerReason Cycle()
		{
			int t_index = (index + 1);

			var reason = NeedsToTriggerScene(t_index);
			if (reason == TriggerReason.Success) // Trigger cutscene
			{
				inprogress = true;
				StartCoroutine(PlayScene(t_index)); // Play scene

				return reason;
			}

			inprogress = false;
			return reason;
		}
		
		#region Scenes

		TriggerReason NeedsToTriggerScene(int t_index)
		{
			if (!Cage.Completed) return TriggerReason.CageNotCompleted;
			if (t_index >= frames.Length) return TriggerReason.SequenceHasCompleted;
		
			var _frames = World.Instance.Profile.weights.behaviours;
			var _frame = Frame.Destruction;
			var _maxFrame = Mathf.NegativeInfinity;
				
			foreach (FrameFloat frame in _frames) 
			{
				if (frame.value > _maxFrame) 
				{
					_maxFrame = frame.value;
					_frame = frame.frame;
				}
			}

			if (preset.overrideSequence) _frame = preset.overrideSequenceFrame; // Override sequence frame (Debug)
				
			frames[t_index] = _frame; // Assign random framing
			return TriggerReason.Success;
		}

		Sequence FetchSequence(Frame frame)
		{
			foreach (Sequence seq in sequences) 
			{
				if (seq.frame == frame)
					return seq;
			}

			return null;
		}

		Scene FetchScene(int index)
		{
			Frame frame = frames[index];
			Sequence sequence = FetchSequence(frame);

			return sequence.Trigger(index);
		}

		IEnumerator PlayScene(int _index)
		{
			Frame frame = frames[_index];
			Scene _scene = FetchScene(_index);
			
			bool didCutscene = Cutscenes.TriggerSequence(_scene);
			if(didCutscene) yield return new WaitForSecondsRealtime(1.3f);

			while (Cutscenes.inprogress) 
				yield return null;

			inprogress = false;
			++index;
		}

		#endregion
	}
}