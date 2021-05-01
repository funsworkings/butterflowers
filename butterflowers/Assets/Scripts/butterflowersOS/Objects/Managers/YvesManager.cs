using System.Linq;
using butterflowersOS.AI;
using butterflowersOS.Interfaces;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace butterflowersOS.Objects.Managers
{
	[ExecuteInEditMode]
	public class YvesManager : MonoBehaviour
	{
		// Properties

		[SerializeField] PostProcessVolume pp;
		Grayscale blulite = null;
		
		[Range(0f, 1f)] public float depth, value, brightness;
		[UnityEngine.Min(0)] public int splits = 0;

		bool active = false;
		public bool IsActive => depth > 0f;

		public bool overrideYves = false;

		void OnEnable()
		{
			active = IsActive;
			
			bool success = pp.profile.TryGetSettings(out blulite);
			Debug.LogWarningFormat("Yves found setting for blulite? " + success); 
		}
		
		void Update()
		{
			if (overrideYves) 
			{
				value = 1f;
				depth = 1f;
			}
			
			blulite.blend.value = value;
			blulite.intensity.value = depth;
			blulite.brightness.value = brightness;
			blulite.tiling.value = 1f / (Mathf.Pow(2f, splits));

			if (active != IsActive) 
			{
				active = IsActive;
				IterateOverElements();
			}
		}

		public void Load(bool didGenerateAgent)
		{
			depth = (didGenerateAgent) ? 1f : 0f;
			value = (didGenerateAgent) ? 1f : 0f;
			brightness = 0f;
			splits = 0;

			active = IsActive;
			
			IterateOverElements();
		}


		void IterateOverElements()
		{
			if (!Application.isPlaying) return;
			
			var elements = FindObjectsOfType<MonoBehaviour>().OfType<IYves>();
			foreach (IYves e in elements) {
				if(IsActive) e.EnableYves();
				else e.DisableYves();
			}
		}
	}
}