using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class SummaryCardTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		// Properties
		
		RectTransform rect;
		SummaryCard card;
		Vector2 size;

		void Awake()
		{
			rect = GetComponent<RectTransform>();
		}
		
		void Start()
		{
			card = GetComponentInParent<SummaryCard>();
			size = rect.sizeDelta;
		}
		
		public void OnPointerEnter(PointerEventData eventData)
		{
			card.Enter();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			card.Exit();
		}
		
		#region Sizing

		public void Grow()
		{
			rect.sizeDelta = (rect.parent as RectTransform).sizeDelta;
		}

		public void Shrink()
		{
			rect.sizeDelta = size;
		}
		
		#endregion
	}
}