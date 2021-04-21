using butterflowersOS.AI;
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


		void Start()
		{
			bool success = pp.profile.TryGetSettings(out blulite);
			Debug.LogWarningFormat("Yves found setting for blulite? " + success);
		}
		
		void Update()
		{
			blulite.blend.value = value;
			blulite.intensity.value = depth;
		}

		public void Load(bool didGenerateAgent)
		{
			depth = (didGenerateAgent) ? 1f : 0f;
			value = (didGenerateAgent) ? 1f : 0f;
		}
	}
}