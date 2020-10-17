using UnityEngine;

namespace AI.Types
{
	[System.Serializable]
	public class ActionSequence 
	{
		public Action[] actions;
		public bool immediate = false;
	}
}