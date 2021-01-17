using UnityEngine;
using Random = UnityEngine.Random;

namespace butterflowersOS.UI.Notifications
{
	public class DiscoveryNotification : GenericNotification
	{
		// Attributes
		
		[SerializeField] AnimationCurve scaleCurve;
		[SerializeField] float scaleTime = 1f;
		[SerializeField] float minRotation = 0f, maxRotation = 30f;
		

		void Start()
		{
			transform.localScale = Vector3.zero;
		}

		protected override void OnInitialize()
		{
			float rotationOffset = Mathf.Sign(Random.Range(-9f, 9f));
			rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(minRotation, maxRotation) * rotationOffset);
			transform.localScale = Vector3.zero;
		}

		protected override void OnUpdate()
		{
			float scale = scaleCurve.Evaluate(Mathf.Clamp01(time / scaleTime));
			transform.localScale = Vector3.one * scale;
		}
	}
}