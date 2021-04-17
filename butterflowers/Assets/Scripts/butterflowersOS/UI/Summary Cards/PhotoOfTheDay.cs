﻿using butterflowersOS.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace butterflowersOS.UI.Summary_Cards
{
	public class PhotoOfTheDay : SummaryCard
	{
		[SerializeField] RawImage imageField = null;
		[SerializeField] TMP_Text captionField = null;

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