using System;
using UnityEngine;

namespace butterflowersOS.AI.Objects
{
	public class Entity : MonoBehaviour
	{
		public static Action<Entity> OnEnabled, OnDisabled, OnDisposed;

		protected virtual void OnEnable()
		{
			OnEnabled?.Invoke(this);
		}

		protected virtual void OnDisable()
		{
			OnDisabled?.Invoke(this);
		}

		protected virtual void OnDestroy()
		{
			OnDisposed?.Invoke(this);
		}
	}
}