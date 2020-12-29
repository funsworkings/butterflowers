using System;
using Data;
using Neue.Reference.Types;
using UnityEngine;
using uwu;

namespace Objects.Managers
{
	public class SequenceManager : MonoBehaviour, ISaveable, IReactToSunCycle
	{
		#region Internal

		[System.Serializable]
		public struct Sequence
		{
			public Frame frame;
			
			public string message;
			public AudioClip audio;
			public GameObject mesh;
		}

		[System.Serializable]
		public class SequenceOption
		{
			public Sequence[] sequences;
		}
		
		#endregion
		
		// External

		Sun Sun;
		
		// Properties

		[SerializeField] int index = -1;
		[SerializeField] Frame[] frames = new Frame[]{};
		
		// Collections

		[SerializeField] SequenceOption[] sequences;
		

		void Start()
		{
			Sun = Sun.Instance;
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
		}
		
		#endregion

		public void Cycle(bool refresh)
		{
			
		}
	}
}