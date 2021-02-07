using System;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.Gameplay
{
	public class PoolObject : MonoBehaviour
	{
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
				return;
			}
			
			Debug.LogWarning("would have destroyed pooled object :(");
		}
	}
}