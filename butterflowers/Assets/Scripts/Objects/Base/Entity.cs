using Interfaces;
using UnityEngine;

namespace Objects.Base
{
	public abstract class Entity : Element, ITooltip
	{
		// Properties

		[SerializeField] AGENT m_agent = AGENT.NULL;

		// Accessors

		public AGENT Agent => m_agent;

		#region Verify update

		protected override bool EvaluateActiveState()
		{
			return Sun.active;
		}

		#endregion

		public virtual string GetInfo()
		{
			return gameObject.name.AppendActionableInformation(this);
		}
	}
}
