using System;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.Gameplay
{
	public class PoolObject : MonoBehaviour
	{
		public UnityEvent onDispose;

		ObjectPool _pool;

		
		public void Initialize(ObjectPool pool)
		{
			_pool = pool;
		}

		public void Dispose()
		{
			if (_pool != null) 
			{
				_pool.Queue(this);
				onDispose.Invoke();
				
				return;
			}
			
			Destroy(gameObject);
		}

		void OnDisable()
		{
			if (_pool != null) 
			{
				_pool.Dequeue(this);
			}
		}

		void OnDestroy()
		{
			if (_pool != null) 
			{
				_pool.Wipe(this);
			}
		}
	}
}