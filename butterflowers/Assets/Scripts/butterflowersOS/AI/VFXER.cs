using System.Collections.Generic;
using butterflowersOS.AI.Objects;
using UnityEngine;
using uwu.Gameplay;

namespace butterflowersOS.AI
{
	public class VFXER : MonoBehaviour
	{
		// Attributes
		
		[SerializeField] List<GameObject> _prefabs = new List<GameObject>();
		
		// Collections
		
		Dictionary<string, ObjectPool> _lookup = new Dictionary<string, ObjectPool>();


		void Start()
		{
			foreach (GameObject prefab in _prefabs) 
			{
				var typeID = prefab.name;

				if (!_lookup.ContainsKey(typeID)) {

					var pool = CreatePool(typeID, prefab);
					_lookup.Add(typeID, pool); // Bind object pool to prefab ID
				}
			}
		}
		
		#region Entities

		ObjectPool CreatePool(string typeID, GameObject prefab)
		{
			var instance = new GameObject(string.Format("Entity_Pool_{0}", typeID));
			instance.transform.parent = transform; // Add as child of VFXER

			var pool = instance.AddComponent<ObjectPool>();
			pool.Prefab = prefab;
			
			return pool;
		}

		public GameObject RequestEntity(string typeID)
		{
			if (_lookup.ContainsKey(typeID)) return _lookup[typeID].Request();
			else return null;
		}
		
		#endregion
	}
}