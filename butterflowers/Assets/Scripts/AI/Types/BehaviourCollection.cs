using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI.Types
{
	[Serializable]
	public class BehaviourCollection<E, J> where J : BehaviourMapping<E>
	{
		[SerializeField] List<J> m_mappings = new List<J>();
		Dictionary<Behaviour, J> m_mappingLookup = null;

		#region Accessors
		
		public List<J> mappings // Populate mappings every time list is accessed
		{
			get { return m_mappings; }
		}

		public Dictionary<Behaviour, J> mappingLookup
		{
			get
			{
				RefreshLookup();
				return m_mappingLookup;
			}
		}

		public bool ContainsBehaviour(Behaviour behaviour)
		{
			return mappingLookup.ContainsKey(behaviour);
		}

		public Behaviour[] behaviours => mappings.Select(map => map.behaviour).ToArray();

		public E GetValue(Behaviour behaviour)
		{
			J map = null;
			mappingLookup.TryGetValue(behaviour, out map);

			if (map == null)
				throw new System.Exception("Unable to find value for behaviour!");

			return map.value;
		}

		public void SetValue(Behaviour behaviour, E value)
		{
			var map = AddProperty(behaviour, value);
			map.value = value;
		}

		#endregion
		
		#region Lookup

		public void RefreshLookup(bool force = false)
		{
			if (m_mappingLookup == null || force) 
			{
				m_mappingLookup = new Dictionary<Behaviour, J>();

				J[] cacheMaps = mappings.ToArray();
				foreach (J map in cacheMaps) 
					AddProperty(map.behaviour, map.value);
			}
		}

		void ScrapeLookup()
		{
			m_mappings = mappingLookup.Values.ToList();
		}

		public J AddProperty(Behaviour behaviour, E value)
		{
			J map = null;
			mappingLookup.TryGetValue(behaviour, out map);
			
			if (map == null) {
				map = (J)Activator.CreateInstance(typeof(J));
				map.behaviour = behaviour;

					AddProperty(map);
			}
			map.value = value;
			
			ScrapeLookup();
			
			return map;

			void AddProperty(J mapping)
			{
				mappings.Add(mapping);
				mappingLookup.Add(behaviour, mapping);
			}
		}

		public void RemoveProperty(Behaviour behaviour)
		{
			J map = null;
			mappingLookup.TryGetValue(behaviour, out map);
			
			if (map != null) {
				mappings.Remove(map);
				mappingLookup.Remove(behaviour);
			}
			
			ScrapeLookup();
		}
		
		#endregion
		
		#region Disposal

		public void Dispose()
		{
			mappingLookup.Clear();
		}
		
		#endregion
	}
}