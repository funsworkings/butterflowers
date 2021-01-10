﻿using System.Collections;
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
		// External

		Sun Sun;
		GameDataSaveSystem _Save;
		
		[SerializeField] Cage Cage;
		[SerializeField] Focusing Focus;
		[SerializeField] ButterflowerManager Butterflowers;
		
		// Properties

		[SerializeField] WorldPreset preset;
		[SerializeField] ToggleOpacity opacity, frameOpacity;
		[SerializeField] TMP_Text frameText;
		[SerializeField] DialogueHandler sceneCaption;
		[SerializeField] AudioSource sceneAudio;

		Sequence[] sequences;

		[SerializeField] int index = -1;
		[SerializeField] Frame[] frames = new Frame[]{};
		[SerializeField] bool inprogress = false, read = false;
		
		// Attributes

		[SerializeField] AnimationCurve lightTransitionInCurve, lightTransitionOutCurve;
		[SerializeField] float lightInTime = 1f, lightOutTime = 1f;
		
		[SerializeField] AnimationCurve meshScaleCurve;
		[SerializeField] float meshScaleTime = 1f;

		[SerializeField] float startDelay = 1f, endDelay = 1f, closeDelay = 1f;
		[SerializeField] float frameDelay = 3f;

		#region Accessors

		public bool Pause => inprogress;
		public bool Read => read;
		
		public bool Complete => (Cage.Completed && (index + 1) < frames.Length);
		
		#endregion
		
		
		void Start()
		{
			Sun = Sun.Instance;
			_Save = GameDataSaveSystem.Instance;

			frames = new Frame[7];
			sequences = GetComponentsInChildren<Sequence>();
			
			sceneCaption.Dispose(); // Clear text in caption
		}

		#region Save/load
		
		public object Save()
		{
			SequenceData seq = new SequenceData();
			seq.index = index;
			seq.frames = frames;
			
			return seq;
		}

		public void Load(object data)
		{
			SequenceData seq = (SequenceData) data;
			if(!preset.persistSequence) 
				seq = new SequenceData();
			
			
			index = seq.index;
			frames = seq.frames;

			if (index >= 0) 
			{
				for (int i = 0; i < index; i++) // Reload all previous scenes
				{
					Frame frame = frames[index];
					Sequence sequence = FetchSequence(frame);
					if (sequence != null) 
					{
						sequence.Trigger(i);
					}
				}
			}
		}
		
		#endregion

		public bool Cycle()
		{
			int t_index = (index + 1);
			
			if (NeedsToTriggerScene(t_index)) // Trigger cutscene
			{
				inprogress = true;
				StartCoroutine(PlayScene(t_index)); // Play scene

				return true;
			}

			inprogress = read = false;
			return false;
		}
		
		#region Scenes

		bool NeedsToTriggerScene(int t_index)
		{
			if (Cage.Completed && t_index < frames.Length) 
			{
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
				
				frames[t_index] = _frame; // Assign random framing
				return true;
			}
			
			return false; 
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

		Sequence.Scene FetchScene(int index)
		{
			Frame frame = frames[index];
			Sequence sequence = FetchSequence(frame);

			return sequence.Trigger(index);
		}

		IEnumerator PlayScene(int _index)
		{
			Frame frame = frames[_index];
			Sequence.Scene _scene = FetchScene(_index);
			
			Vector3 baseMeshScale = _scene.mesh.transform.localScale;
			_scene.mesh.transform.localScale = Vector3.zero;
			
			opacity.Show();
			
			float lt = 0f;
			while (!SmoothLight(ref lt, false))
				yield return null;
			
			yield return new WaitForSecondsRealtime(startDelay);
			
			float st = 0f;
			while (!ScaleMesh(ref st, _scene.mesh, baseMeshScale))
				yield return null;
			
			yield return new WaitForSecondsRealtime(endDelay);

			bool didFocusOnMesh = false;

			var focusable = _scene.mesh.GetComponent<Focusable>();
			if (focusable != null) 
			{
				didFocusOnMesh = focusable.Focus(); // Set focus to mesh focus!
			}

			lt = 0f;
			while (!SmoothLight(ref lt, true)) 
				yield return null;

			//string debugMessage = "It is {0} on the {1}th day in the year of our Lord, 2020";
			//sceneCaption.Push(string.Format(debugMessage, Enum.GetName(typeof(Frame), frames[_index]).ToUpper(), _index));

			var framing = System.Enum.GetName(typeof(Frame), frame).ToUpper();
			frameText.text = framing;
			
			frameOpacity.Show();
			while (!frameOpacity.Visible) yield return null;
			yield return new WaitForSecondsRealtime(frameDelay);
			frameOpacity.Hide();

			sceneCaption.Push(_scene.message);
			if (_scene.audio != null) // Play scene audio!
			{
				sceneAudio.clip = _scene.audio;
				sceneAudio.Play();
			}
			
			while (sceneCaption.inprogress || sceneAudio.isPlaying) 
			{
				read = true;
				inprogress = false;
				
				yield return null;
			}

			inprogress = read = false;
			opacity.Hide();
			++index;
		}

		#region Ops

		bool SmoothLight(ref float lt, bool _in)
		{
			float duration = (_in) ? lightInTime : lightOutTime;
			AnimationCurve curve = (_in) ? lightTransitionInCurve : lightTransitionOutCurve;
			
			lt += Time.unscaledDeltaTime;
			
			var li = Mathf.Clamp01(lt / duration);
			Sun.intensity = curve.Evaluate(li);

			return li >= 1f;
		}

		bool ScaleMesh(ref float st, GameObject mesh, Vector3 scale)
		{
			st += Time.unscaledDeltaTime;
			
			var si = Mathf.Clamp01(st / meshScaleTime);
			var sc = scale * meshScaleCurve.Evaluate(si);

			mesh.transform.localScale = sc;

			return si >= 1f;
		}
		
		#endregion
		
		#endregion
	}
}