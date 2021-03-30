using System;
using TMPro;
using UnityEngine;

namespace butterflowersOS.Miscellaneous
{
	public class FramesPerSecond : MonoBehaviour
	{
		public int FPS { get; set; }

		TMP_Text textField;

		void Awake()
		{
			textField = GetComponent<TMP_Text>();
		}

		void Update()
		{
			FPS = (int)(1f / Time.unscaledDeltaTime);
			textField.text = FPS + "";
		}
	}
}