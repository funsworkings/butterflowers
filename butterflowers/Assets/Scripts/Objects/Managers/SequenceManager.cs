using System;
using System.Collections;
using Data;
using Neue.Reference.Types;
using Objects.Entities;
using TMPro;
using UnityEngine;
using uwu;
using uwu.Dialogue;
using Random = UnityEngine.Random;

namespace Objects.Managers
{
	public class SequenceManager : MonoBehaviour, ISaveable, IReactToSunCycle
	{
		// External

		Sun Sun;
		
		// Properties

		[SerializeField] DialogueHandler sceneCaption;
		[SerializeField] AudioSource sceneAudio;

		Sequence[] sequences;

		[SerializeField] int index = -1;
		[SerializeField] Frame[] frames = new Frame[]{};
		[SerializeField] bool inprogress = false;
		
		// Attributes

		[SerializeField] AnimationCurve lightTransitionInCurve, lightTransitionOutCurve;
		[SerializeField] float lightInTime = 1f, lightOutTime = 1f;
		
		[SerializeField] AnimationCurve meshScaleCurve;
		[SerializeField] float meshScaleTime = 1f;
		

		void Start()
		{
			Sun = Sun.Instance;

			frames = new Frame[7];
			sequences = GetComponentsInChildren<Sequence>();
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

		public void Cycle(bool refresh)
		{
			if (NeedsToTriggerScene()) // Trigger cutscene
			{
				StartCoroutine(PlayScene()); // Play scene
			}
		}
		
		#region Scenes

		bool NeedsToTriggerScene()
		{
			if (++index < frames.Length) 
			{
				frames[index] = (Frame)(Random.Range(0, 4)); // Assign random framing
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

		Sequence.Scene FetchScene()
		{
			Frame frame = frames[index];
			Sequence sequence = FetchSequence(frame);

			return sequence.Trigger(index);
		}

		IEnumerator PlayScene()
		{
			inprogress = true;
			
			Sequence.Scene _scene = FetchScene();
			
			Vector3 baseMeshScale = _scene.mesh.transform.localScale;
			_scene.mesh.transform.localScale = Vector3.zero;
			
			float lt = 0f;
			while (!SmoothLight(ref lt, false))
				yield return null;
			
			float st = 0f;
			while (!ScaleMesh(ref st, _scene.mesh, baseMeshScale))
				yield return null;

			lt = 0f;
			while (!SmoothLight(ref lt, true)) 
				yield return null;
			
			
			sceneCaption.Push(_scene.Message);
			
			inprogress = false;
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