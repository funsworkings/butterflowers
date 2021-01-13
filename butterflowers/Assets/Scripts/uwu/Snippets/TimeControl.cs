using System;
using UnityEngine;

namespace uwu.Snippets
{
	public class TimeControl : MonoBehaviour
	{
		[SerializeField] float speed = 1f;
		[SerializeField] float adjustSpeed = 1f;
		
		void Update()
		{
			float dt = Time.unscaledDeltaTime;

			speed = Mathf.Max(0f, speed);
			Time.timeScale = speed;
		}
	}
}