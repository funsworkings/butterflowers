using System;
using UnityEngine;
using uwu.Gameplay;
using Object = System.Object;

namespace butterflowersOS.AI.Objects
{
	public class Entity : MonoBehaviour
	{
		public static Action<Entity> OnEnabled, OnDisabled, OnDisposed;
		public System.Action<Vector3> onWrap;

		PoolObject _pooler;
		CCTV _cctv;
		
		[SerializeField] float lifetime = -1f;
		[SerializeField] bool _waitForTrigger = true;
		
		float life = 0f;

		Vector3 _wrapPosition;
		
		bool _didWrap = false;
		bool _wrapDuringFrame = false;
		bool _ready = false;

		protected bool ready => _ready;
		protected bool waitForTrigger => _waitForTrigger;

		public bool WillWrapDuringFrame => _wrapDuringFrame;
		
		protected virtual void Awake()
		{
			_pooler = GetComponent<PoolObject>();
			if (_pooler == null) {
				Debug.LogWarningFormat("Pooler has not been assigned for {0}, one will be created", gameObject.name);
				_pooler = gameObject.AddComponent<PoolObject>();
			}

			_cctv = FindObjectOfType<CCTV>();
		}

		protected virtual void OnEnable()
		{
			OnEnabled?.Invoke(this);

			life = lifetime; // Reset life
			_ready = !waitForTrigger;
			
			if(_cctv != null) OrientToCamera();
		}

		protected virtual void OnDisable()
		{
			_didWrap = _wrapDuringFrame = false; // Clear wrapping behaviours
			
			if(_pooler != null) _pooler.Dispose(); // Dispose pooled object
			
			OnDisabled?.Invoke(this);
		}

		protected virtual void Update()
		{
			if (!ready) return; // Wait until ready to fire
			
			if (_didWrap) 
			{
				if (_wrapDuringFrame) PostWrap();
				else _wrapDuringFrame = true;
			}
			
			if(lifetime > 0f) Decay();
		}

		protected virtual void OnDestroy()
		{
			OnDisposed?.Invoke(this);
		}

		public virtual void Trigger(float saturation, float value, Color baseline, params object[] data)
		{
			_ready = true;
		}
		
		#region Wrapping

		public void PreWrap(Vector3 from, Vector3 to)
		{
			if (_didWrap) return; // Ignore duplicate wrap requests

			_wrapPosition = to;
			_didWrap = true;

			OnPreWrap(from, to);
		}

		protected void PostWrap()
		{
			transform.position = _wrapPosition;
			_didWrap = _wrapDuringFrame = false;
			
			onWrap?.Invoke(_wrapPosition);
			
			OnPostWrap();
		}
		
		// Virtual callbacks for wrap operations
		
		protected virtual void OnPreWrap(Vector3 from, Vector3 to){}
		protected virtual void OnPostWrap(){}
		
		#endregion
		
		#region Lifetime

		void Decay()
		{
			life -= Time.deltaTime;
			if (life <= 0f) 
			{
				_pooler.Dispose(); // Dispose entity
			}
		}
		
		#endregion
		
		#region Camera

		protected void OrientToCamera()
		{
			Vector3 direction = (_cctv.transform.position - transform.position).normalized;
			DidOrientToCamera(direction);
		}

		protected virtual void DidOrientToCamera(Vector3 direction)
		{
			transform.forward = direction;
		}

		#endregion
	}
}