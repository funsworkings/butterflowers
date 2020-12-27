using System;

namespace Neue.Types
{
	[Obsolete("Obsolete API!", true)]
	[System.Serializable]
	public class ActionSequence 
	{
		public Action[] actions;
		public bool immediate = false;
	}
}