using AI.Types.Mappings;
using Noder.Graphs;
using UnityEngine;

namespace AI.Types
{
	[System.Serializable]
	public class Action
	{
		public System.Action onCancel, onComplete;
		
		public EVENTCODE @event = EVENTCODE.NULL;
		public Transform root = null;
		public ModuleTree tree = null;
		public object dat = null;
		public BehaviourIntGroup rewards = new BehaviourIntGroup();
		
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