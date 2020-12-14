using System;
using System.Collections.Generic;
using Neue.Types;

namespace Neue.Reference.Types
{
	[System.Serializable]
	public class FrameMapGroup<E, J> where J:FrameMap<E>
	{
		public List<J> behaviours = new List<J>();
		public E defaultValue = default(E);
		
		/* * * * * * * * * * * * * * */

		public bool HasValue(Frame frame)
		{
			foreach (J b in behaviours) {
				if (b.frame == frame)
					return true;
			}

			return false;
		}

		public E GetValue(Frame frame)
		{
			foreach (J b in behaviours) {
				if (b.frame == frame)
					return b.value;
			}

			return defaultValue;
		}

		public void SetValue(Frame frame, E value, bool addIfDoesntExist = false)
		{
			foreach (J b in behaviours) {
				if (b.frame == frame) {
					b.value = value;
					return;
				}
			}

			if (addIfDoesntExist) 
			{
				var map = (J) Activator.CreateInstance(typeof(J));
					map.frame = frame;
					map.value = value;
					
				behaviours.Add(map);
			}
		}
	}
}