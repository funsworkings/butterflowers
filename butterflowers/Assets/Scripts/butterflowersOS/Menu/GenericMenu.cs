using System;
using UnityEngine;

namespace butterflowersOS.Menu
{
	public abstract class GenericMenu : MonoBehaviour
	{
		[SerializeField] 
		protected bool visible = false;
		
		public bool IsVisible { get { return visible; } }


		protected virtual void Start()
		{
			if (visible) Open();
			else Close();
		}

		#region Ops

		public void Open()
		{
			visible = true;
			DidOpen();
		}

		public void Close()
		{
			visible = false;
			DidClose();
		}
		
		public void Toggle()
		{
			if (visible) Close();
			else Open();
		}

		#endregion
		
		#region Abstract members
		
		protected abstract void DidOpen();
		protected abstract void DidClose();
		
		#endregion
	}
}