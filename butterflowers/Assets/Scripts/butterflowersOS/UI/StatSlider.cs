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

		[SerializeField] Image bar;
		
		// Attributes
		
		public Frame frame;

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
	}
}