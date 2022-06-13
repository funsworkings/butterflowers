using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR

#endif

namespace uwu.Snippets
{
	[ExecuteInEditMode]
	public class Line : MonoBehaviour
	{
		[SerializeField] RectTransform origin = null, destination = null;

		[SerializeField] GameObject dotPrefab = null;
		[SerializeField] List<LineDot> dots = new List<LineDot>();

		public bool active = true;
		public bool dotted;

		[Header("Dotted Attributes")]
		//[Range(0f, 1f)] 
		//public float dotDistribution = 1f;
		public float dotSize = 32f;

		Image image;

		int numberOfDots, previousNumberOfDots;

		RectTransform rect;

		public float length => rect.sizeDelta.y;

		public Vector2 start => new Vector2(0f, -length / 2f);

		public Vector2 end => new Vector2(0f, length / 2f);


		void Update()
		{
			if (rect == null)
				rect = GetComponent<RectTransform>();
			if (image == null)
				image = GetComponent<Image>();


			var clearFlag = true;

			if (active)
				if (origin != null && destination != null) {
					var dir = destination.anchoredPosition - origin.anchoredPosition;
					var sa = destination.sizeDelta.magnitude / 2f;
					var sb = origin.sizeDelta.magnitude / 2f;

					rect.anchoredPosition = (origin.anchoredPosition + destination.anchoredPosition) / 2f -
					                        dir.normalized * (sa - sb) / 2f;

					rect.sizeDelta = new Vector2(dotSize / 2f, Mathf.Max(0f, dir.magnitude - sa - sb));

					transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f);

					clearFlag = false;
				}

			if (!clearFlag) {
				clearFlag = true;

				if (dotted) {
					image.enabled = false;

					if (dotPrefab != null) {
						dotSize = Mathf.Max(0f, dotSize); // Ensure dot size is always >= 0

						previousNumberOfDots = transform.childCount; // Settle previous number of dots with current
						numberOfDots = CalculateNumberOfDots();

						if (numberOfDots > previousNumberOfDots)
							AddDots(numberOfDots - previousNumberOfDots);
						else if (numberOfDots < previousNumberOfDots)
							RemoveDots(previousNumberOfDots - numberOfDots);

						dots = GetComponentsInChildren<LineDot>().ToList();

						if (dots.Count > 0) {
							var index = 0;
							var interval = 0f;

							var pos = start;
							var tempDots = dots.ToArray();
							for (var i = 0; i < tempDots.Length; i++) {
								var ld = tempDots[i];
								if (ld != null) {
									index = ld.transform.GetSiblingIndex();
									interval = index * 1f / numberOfDots;

									pos = start + (end - start).normalized * interval * length;
									ld.x = pos.x;
									ld.y = pos.y;
									ld.radius = dotSize;
								}
								else {
									dots.RemoveAt(i);
								}
							}
						}

						clearFlag = false;
					}
				}
				else {
					image.enabled = true;
				}
			}
			else {
				image.enabled = false;
			}

			if (clearFlag)
				ClearDots();
		}

		int CalculateNumberOfDots()
		{
			var maxDots = Mathf.FloorToInt(length / dotSize);

			var space = Mathf.Max(0f, length);
			var numDots = space < dotSize ? 0 : Mathf.FloorToInt(space / dotSize);

			return numDots;
		}

		void AddDots(int amount)
		{
			GameObject dot_o;
			LineDot dot_sc;

			for (var i = 0; i < amount; i++) {
				dot_o = Instantiate(dotPrefab, transform);
				dot_sc = dot_o.GetComponent<LineDot>();

				dots.Add(dot_sc);
			}
		}

		void RemoveDots(int amount)
		{
			for (var i = 0; i < amount; i++)
				if (dots.Count >= i) {
					if (dots[i] != null) {
#if UNITY_EDITOR
						if (EditorApplication.isPlaying)
							Destroy(dots[i].gameObject);
						else
							DestroyImmediate(dots[i].gameObject);
#else
                        GameObject.Destroy(dots[i].gameObject);
#endif
					}

					dots.RemoveAt(0);
				}
		}

		void ClearDots()
		{
			dots = GetComponentsInChildren<LineDot>().ToList();

			while (dots.Count > 0) {
#if UNITY_EDITOR
				if (EditorApplication.isPlaying)
					Destroy(dots[0].gameObject);
				else
					DestroyImmediate(dots[0].gameObject);
#else
                GameObject.Destroy(dots[0].gameObject);
#endif

				dots.RemoveAt(0);
			}
		}
	}
}