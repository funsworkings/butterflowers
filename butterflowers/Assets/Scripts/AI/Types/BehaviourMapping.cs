﻿using UnityEngine;

namespace AI.Types
{
	[System.Serializable]
	public class BehaviourMapping<E>
	{
		public Behaviour behaviour;
		public E value;
		
		public BehaviourMapping(){}
	}
}