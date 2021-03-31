using System;
using TMPro;
using UnityEngine;

namespace butterflowersOS.Miscellaneous
{
	public class FramesPerSecond : MonoBehaviour
	{
		public int FPS { get; set; }

		[SerializeField] TMP_Text[] textFields;

		void Update()
		{
			FPS = (int)(1f / Time.unscaledDeltaTime);
			foreach(TMP_Text textField in textFields) textField.text = FPS + "";
		}
	}
}