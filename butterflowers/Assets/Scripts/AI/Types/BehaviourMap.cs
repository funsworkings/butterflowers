using UnityEngine;

namespace AI.Types
{
	[System.Serializable]
	public class BehaviourMap<E>
	{
		public Behaviour behaviour;
		public E value;
		
		public BehaviourMap(){}
	}
}