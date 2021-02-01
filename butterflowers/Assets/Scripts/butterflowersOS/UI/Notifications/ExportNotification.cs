using UnityEngine;
using Random = UnityEngine.Random;

namespace butterflowersOS.UI.Notifications
{
	public class ExportNotification : GenericNotification
	{
		// Attributes
		
		[SerializeField] AnimationCurve scaleCurve;
		[SerializeField] float scaleTime = 1f;
		

		void Start()
		{
			transform.localScale = Vector3.zero;
		}

		protected override void OnInitialize()
		{
			transform.localScale = Vector3.zero;
		}

		protected override void OnUpdate()
		{
			float scale = scaleCurve.Evaluate(Mathf.Clamp01(time / scaleTime));
			transform.localScale = Vector3.one * scale;
		}
	}
}