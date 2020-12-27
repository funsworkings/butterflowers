using System;
using Neue.Reference.Types.Maps.Groups;
using Noder.Graphs;
using UnityEngine;

namespace Neue.Types
{
	[Obsolete("Obsolete API!", true)]
	[System.Serializable]
	public class Action
	{
		public System.Action onCancel, onComplete;
		
		public EVENTCODE @event = EVENTCODE.NULL;
		public Transform root = null;
		public ModuleTree tree = null;
		public object dat = null;
		public FrameIntGroup rewards = new FrameIntGroup();
		
		public bool immediate = false;
		public float delay = 0f;
		public bool auto = false;

		public void Complete()
		{
			if (onComplete != null)
				onComplete();
		}

		public void Cancel()
		{
			if (onCancel != null)
				onCancel();
		}
	}
}