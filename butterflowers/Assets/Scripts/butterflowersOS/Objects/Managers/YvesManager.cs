using System.Linq;
using butterflowersOS.AI;
using butterflowersOS.Interfaces;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace butterflowersOS.Objects.Managers
{
	public class YvesManager : MonoBehaviour
	{
		// Properties

		[SerializeField] PostProcessVolume pp;
		Grayscale blulite = null;
		
		[SerializeField, Range(0f, 1f)] float depth, value;


		bool active = false;
		public bool IsActive => depth > 0f;

		void Start()
		{
			active = IsActive;
			
			bool success = pp.profile.TryGetSettings(out blulite);
			Debug.LogWarningFormat("Yves found setting for blulite? " + success);
		}
		
		void Update()
		{
			blulite.blend.value = value;
			blulite.intensity.value = depth;

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
			
			IterateOverElements();
		}


		void IterateOverElements()
		{
			var elements = FindObjectsOfType<MonoBehaviour>().OfType<IYves>();
			foreach (IYves e in elements) {
				if(IsActive) e.EnableYves();
				else e.DisableYves();
			}
		}
	}
}