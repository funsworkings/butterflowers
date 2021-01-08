using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Summary_Cards
{
	public class PhotoOfTheDay : SummaryCard
	{
		[SerializeField] RawImage imageField;
		[SerializeField] TMP_Text captionField;

		public RawImage Image => imageField;

		protected override void Start()
		{
			deck = GetComponentInParent<SummaryDeck>();
			trigger = GetComponentInChildren<SummaryCardTrigger>();
			
			normalScale = rect.localScale;
			focusScale.Scale(normalScale);
		}

		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score){}

		public void ShowPhoto(Texture2D image, string caption)
		{
			imageField.texture = image;
			captionField.text = caption;
		}
	}
}