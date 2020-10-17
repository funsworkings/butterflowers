﻿using UnityEngine;

namespace uwu.Text.Effects
{
	public class Text_WiggleEffect : TextEffect
	{
		public float magnitude = 1f;

		[SerializeField] float wavelength = 6.28f;
		[SerializeField] float speed = 1f;


		protected override void ComputeTranslationPerVertex(ref Vector3 offset, int index, int length)
		{
			var interval = index / 10f;
			var period = 2f * Mathf.PI / wavelength;

			var len = magnitude * Mathf.Sin((Time.time * speed + interval * wavelength) / period);
			offset = Vector3.up * len;
		}
	}
}