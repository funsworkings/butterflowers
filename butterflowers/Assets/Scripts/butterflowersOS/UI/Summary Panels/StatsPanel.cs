using System;
using butterflowersOS.Core;
using Neue.Agent.Brain.Data;
using UnityEngine;

namespace butterflowersOS.UI.Summary_Panels
{
	public class StatsPanel : SummaryPanel
	{
		WindowResize resizer;
		
		World World = null;
		StatSlider[] _sliders = new StatSlider[]{};
		
		// Attributes

		[SerializeField] float fillSpeed = 1f;

		void OnEnable()
		{
			resizer.onResize += WindowDidResize;
		}

		void OnDisable()
		{
			resizer.onResize -= WindowDidResize;
		}

		protected override void Start()
		{
			base.Start();

			World = FindObjectOfType<World>();
			_sliders = GetComponentsInChildren<StatSlider>();
		}

		protected override void OnShown()
		{
			WindowDidResize(0, 0);
			TriggerSliders();
		}

		void TriggerSliders()
		{
			Profile cacheProfile = World.Profile;

			foreach (StatSlider slider in _sliders) 
			{
				float weight = cacheProfile.GetWeight(slider.frame);
				slider.Trigger(weight, fillSpeed);
			}
		}

		void WindowDidResize(int width, int height)
		{
			foreach (StatSlider slider in _sliders) 
			{
				slider.Refresh();
			}
		}
	}
}