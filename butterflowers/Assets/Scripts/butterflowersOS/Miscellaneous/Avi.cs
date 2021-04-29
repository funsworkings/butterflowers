using System;
using System.Collections;
using UnityEngine;

namespace butterflowersOS.Miscellaneous
{
	public class Avi : MonoBehaviour
	{
		
		// Properties

		[SerializeField] GameObject hat, glasses;

		void Start()
		{
			Hide();
		}

		public void Show(int layer)
		{
			hat.SetActive(true);
			glasses.SetActive(true);

			hat.layer = glasses.layer = layer;
		}

		public void Hide()
		{
			hat.SetActive(false);
			glasses.SetActive(false);
		}
	}
}