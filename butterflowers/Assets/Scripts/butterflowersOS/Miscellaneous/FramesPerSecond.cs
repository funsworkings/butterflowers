using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace butterflowersOS.Miscellaneous
{
	public class FramesPerSecond : MonoBehaviour
	{
		public int FPS { get; set; }

		[SerializeField] TMP_Text[] textFields;
		[SerializeField] float refreshRate = 1f;

		void Start()
		{
			StartCoroutine("Loop");
		}

		void OnDestroy()
		{
			StopAllCoroutines();
		}

		IEnumerator Loop()
		{
			while (true) 
			{
				FPS = (int)(1f / Time.unscaledDeltaTime);
				foreach(TMP_Text textField in textFields) textField.text = FPS + "";
				
				yield return new WaitForSeconds(refreshRate);
			}
		}
	}
}