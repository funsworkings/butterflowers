using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace butterflowersOS.UI.Notifications
{
	public class ExportNotification : GenericNotification, IPointerEnterHandler, IPointerExitHandler
	{
		// Attributes
		
		[SerializeField] AnimationCurve scaleCurve;
		[SerializeField] float scaleTime = 1f;

		int interact = 0;

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

		protected override void Clear()
		{
			if (interact <= 1) return;
			base.Clear();
		}

		#region Pointer events

		public void OnPointerEnter(PointerEventData eventData)
		{
			interact++;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			interact++;
		}

		#endregion
	}
}