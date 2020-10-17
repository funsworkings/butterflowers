using UnityEngine;

namespace uwu.Generic
{
	/// <summary>
	///     Generic class for attributes that are driven by a controller (Player, Capture, etc)
	/// </summary>
	public abstract class Attribute<T> : MonoBehaviour where T : MonoBehaviour
	{
		[SerializeField] protected bool active = true;
		protected T controller;

		public bool Active
		{
			get => active && controller != null && controller.enabled;
			set
			{
				active = value;

				if (active)
					onActive();
				else
					onInactive();
			}
		}


		protected void Awake()
		{
			controller = GetComponent<T>(); // Fetch controller
			onAwake();
		}


		protected void Start()
		{
			if (!Active)
				return;

			onStart();
		}


		protected void Update()
		{
			if (!Active) // Halt all update commands if not active
				return;

			onUpdate();
		}


		protected void OnDestroy()
		{
			if (!Active)
				return;

			onDestroy();
		}


		protected virtual void onActive()
		{
		}

		protected virtual void onInactive()
		{
		}

		protected virtual void onAwake()
		{
		}

		protected virtual void onStart()
		{
		}

		protected virtual void onUpdate()
		{
		}

		protected virtual void onDestroy()
		{
		}


		public virtual void Enable()
		{
			active = true;
		}

		public virtual void Disable()
		{
			active = false;
		}

		public virtual void Toggle()
		{
			if (active) Disable();
			else Enable();
		}
	}
}