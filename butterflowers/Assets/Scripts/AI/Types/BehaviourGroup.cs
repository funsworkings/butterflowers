using System;
using System.Collections.Generic;

namespace AI.Types
{
	[System.Serializable]
	public class BehaviourGroup<E, J> where J:BehaviourMap<E>
	{
		public List<J> behaviours = new List<J>();
		public E defaultValue = default(E);
		
		/* * * * * * * * * * * * * * */

		public bool HasValue(Behaviour behaviour)
		{
			foreach (J b in behaviours) {
				if (b.behaviour == behaviour)
					return true;
			}

			return false;
		}

		public E GetValue(Behaviour behaviour)
		{
			foreach (J b in behaviours) {
				if (b.behaviour == behaviour)
					return b.value;
			}

			return defaultValue;
		}

		public void SetValue(Behaviour behaviour, E value, bool addIfDoesntExist = false)
		{
			foreach (J b in behaviours) {
				if (b.behaviour == behaviour) {
					b.value = value;
					return;
				}
			}

			if (addIfDoesntExist) 
			{
				var map = (J) Activator.CreateInstance(typeof(J));
					map.behaviour = behaviour;
					map.value = value;
					
				behaviours.Add(map);
			}
		}
	}
}