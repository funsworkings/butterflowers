using System;
using UnityEngine;

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
			
			Destroy(gameObject);
		}
	}
}