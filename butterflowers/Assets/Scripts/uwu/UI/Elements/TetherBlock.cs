using System;
using uwu.Behaviors;

namespace uwu.UI.Elements
{
	public class TetherBlock<E> : ActionBlock, ITether<E>
	{
		public static Action<E> OnSelectTether;

		E m_tether;

		public E tether
		{
			get => m_tether;
			set
			{
				m_tether = value;
				OnUpdateTether(tether);
			}
		}


		protected virtual void OnUpdateTether(E tether)
		{
		}

		protected override void InvokeAction()
		{
			if (OnSelectTether != null)
				OnSelectTether(tether);
		}
	}
}