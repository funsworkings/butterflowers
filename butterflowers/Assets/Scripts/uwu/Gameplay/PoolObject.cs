using System;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.Gameplay
{
	public class PoolObject : MonoBehaviour
	{
		ObjectPool _pool;

		bool _applicationWillQuit = false;

		void OnApplicationQuit()
		{
			_applicationWillQuit = true;
		}

		public void Initialize(ObjectPool pool)
		{
			_pool = pool;
		}

		public void Dispose()
		{
			if (_pool != null) 
			{
				_pool.Queue(this);
				return;
			}
			
			Debug.LogWarning("would have destroyed pooled object :(");
		}

		void OnDestroy()
		{
			if (!_applicationWillQuit) 
			{
				Debug.LogWarning("Destroyed a pooled object, should be returned to the object pool!");	
			}
		}
	}
}