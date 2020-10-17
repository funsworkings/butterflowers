using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.Snippets
{
	[RequireComponent(typeof(UnityEngine.Camera))]
	public class Snapshot : MonoBehaviour
	{
		public UnityEvent onStart, onEnd;
		bool inprogress;

		public Action<Texture2D> onSuccess;

		public new UnityEngine.Camera camera { get; set; }

		void Awake()
		{
			camera = GetComponent<UnityEngine.Camera>();
		}

		public void Capture()
		{
			if (!inprogress) {
				StartCoroutine("Capturing");
				inprogress = true;
			}
		}

		IEnumerator Capturing()
		{
			yield return new WaitForEndOfFrame();

			var w = Screen.width;
			var h = Screen.height;

			var screenshot = new Texture2D(w, h, TextureFormat.RGB24, false);

			var rt = new RenderTexture(w, h, 24);

			camera.targetTexture = rt;
			camera.Render();

			var prt = RenderTexture.active;
			RenderTexture.active = rt;

			screenshot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
			screenshot.Apply();

			camera.targetTexture = null;
			RenderTexture.active = prt;

			Destroy(rt);

			onStart.Invoke();
			yield return new WaitForEndOfFrame();
			onEnd.Invoke();

			inprogress = false;
			if (onSuccess != null)
				onSuccess(screenshot);
		}
	}
}