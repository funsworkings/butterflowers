using System;
using System.Collections;
using Neue.Reference.Types;
using UnityEngine;
using UnityEngine.UI;

namespace butterflowersOS.UI
{
	public class StatSlider : MonoBehaviour
	{
		// Properties

		RectTransform _rect;
		HorizontalLayoutGroup _layoutGroup;

		[SerializeField] LayoutElement barLayout, textLayout;
		[SerializeField] Image bar;
		
		// Attributes
		
		public Frame frame;

		[SerializeField, Range(0f, 1f)] float barPercentage = .75f;

		void Awake()
		{
			_rect = GetComponent<RectTransform>();
			_layoutGroup = GetComponent<HorizontalLayoutGroup>();
		}

		public void Trigger(float weight, float speed)
		{
			bar.fillAmount = 0f;
			StartCoroutine(Fill(weight, speed));
		}

		IEnumerator Fill(float weight, float speed)
		{
			float w = 0f;
			while (w < weight) 
			{
				w += Time.unscaledDeltaTime * speed;
				if (w > weight) w = weight;

				bar.fillAmount = w;
				yield return null;
			}
		}

		public void Refresh()
		{
			float width = _rect.sizeDelta.x - _layoutGroup.spacing;

			barLayout.preferredWidth = barPercentage * width;
			textLayout.preferredWidth = (1f - barPercentage) * width;
		}
	}
}