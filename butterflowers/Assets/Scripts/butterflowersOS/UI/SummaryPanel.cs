using System;
using UnityEngine;
using UnityEngine.Events;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.UI
{
	public class SummaryPanel : MonoBehaviour
	{
		public UnityEvent onActive, onInactive;
		
		// Properties

		protected ToggleOpacity _opacity;

		protected virtual void Awake()
		{
			_opacity = GetComponent<ToggleOpacity>();
		}

		protected virtual void Start()
		{
			Hide(); // Hidden by default
		}

		public void Show()
		{
			_opacity.Show();
			onActive.Invoke();
			
			OnShown();
		}

		public void Hide()
		{
			_opacity.Hide();
			onInactive.Invoke();
			
			OnHidden();
		}
		
		protected virtual void OnShown(){}
		protected virtual void OnHidden(){}
	}
}