using UnityEngine;

namespace Neue.Agent.Types
{
	public abstract class Module : MonoBehaviour
	{
		// Properties
		
		[SerializeField] protected Base @base;

		[SerializeField] float weight = 1f;
		[SerializeField] int priority = 0;

		#region Accessors

		public float _Weight { get => weight; private set { weight = Mathf.Clamp01(value);} }
		public int _Priority { get => priority; set { priority = value; } }
		
		#endregion

		#region Operations

		public virtual void Init(Base @base){ this.@base = @base; }
		public abstract void Continue();
		public abstract void Pause();
		public abstract void Destroy();

		#endregion
	}
}