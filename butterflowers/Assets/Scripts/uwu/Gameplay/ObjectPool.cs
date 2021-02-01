using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace uwu.Gameplay
{
	public class ObjectPool : MonoBehaviour
	{
		// Collections
		
		[SerializeField] List<PoolObject> active = new List<PoolObject>();
		[SerializeField] List<PoolObject> pool = new List<PoolObject>();
		
		// Properties

		[SerializeField] GameObject prefab;
		
		// Attributes

		[SerializeField] int initialSpawnAmount = 0;



		void Start()
		{
			if (initialSpawnAmount > 0) 
			{
				for (int i = 0; i < initialSpawnAmount; i++) Queue(Spawn());
			}
		}
		
		
		#region Ops
		
		public GameObject Request()
		{
			return Request(1).ElementAt(0);
		}

		public GameObject[] Request(int amount)
		{
			List<GameObject> payload = new List<GameObject>();

			for (int i = 0; i < amount; i++) 
			{
				payload.Add(Dequeue().gameObject);
			}

			return payload.ToArray();
		}
		
		#endregion

		#region Spawning
		
		PoolObject Spawn()
		{
			PoolObject @poolObject = null;
			var instance = Instantiate(prefab);

			@poolObject = instance.GetComponent<PoolObject>();
			if (@poolObject == null) @poolObject = instance.AddComponent<PoolObject>();
				
			@poolObject.Initialize(this);
			return @poolObject;
		}

		#endregion

		#region Queue
		
		public void Queue(PoolObject @object)
		{
			if (pool.Contains(@object)) return;

			if (active.Contains(@object)) active.Remove(@object);
			pool.Add(@object);

			@object.transform.parent = transform; // Reset transform of queued pool object
			@object.gameObject.SetActive(false);
		}
		
		PoolObject Dequeue()
		{
			PoolObject @object = null;
			PoolObject[] cache_pool = pool.ToArray();
			
			if (cache_pool.Length == 0) 
			{
				@object = Spawn(); // Spawn new pooled instance!
			}
			else 
			{
				@object = cache_pool[0];	
				pool.RemoveAt(0);
			}

			active.Add(@object);

			@object.transform.parent = null; // Release from object pool
			@object.gameObject.SetActive(true);
			
			return @object;
		}

		#endregion
	}
}