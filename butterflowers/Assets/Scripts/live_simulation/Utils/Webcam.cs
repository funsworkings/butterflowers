using System;
using System.Collections;
using UnityEngine;
using uwu.Textures;

namespace live_simulation.Utils
{
    public class Webcam : MonoBehaviour
	{
		// Properties
		
		[SerializeField] string deviceName;
		[SerializeField] int requestWidth = 400, requestHeight = 300, requestFPS = 12;

		WebCamTexture wct;
		public WebCamTexture LiveTexture => wct;

		public bool Ready { get; private set; } = false;
		public bool Playing => wct.isPlaying;
		
		void Start()
		{
			WebCamDevice? t_device = null;
			
			var devices = WebCamTexture.devices;
			foreach (WebCamDevice device in devices) 
			{
				Debug.Log($"Discovered video device: {device.name}");
				if (device.name == deviceName) {
					t_device = device;
					break;
				}
			}

			if (t_device.HasValue) 
			{
				Ready = true;

				var device = t_device.Value;
				Debug.Log($"Trigger {device.name}");

				try {
					var _resolution = FindLowestResolutionForDevice(device);
					wct = new WebCamTexture(device.name, _resolution.width, _resolution.height,
						_resolution.refreshRate); //, requestWidth, requestHeight, requestFPS);
				}
				catch (System.Exception err) {
					Debug.LogWarning(err.Message);
					wct = new WebCamTexture(device.name);
				}

				wct.Play();
			}
		}

		public void Capture(int width, int height, System.Action<Texture2D> onCompleted)
		{
			if(!Ready) throw new System.Exception("Device not ready!");

			Texture2D render=  new Texture2D(wct.width, wct.height); 
			render.SetPixels32(wct.GetPixels32());
			render.Apply();

			TextureScale.Bilinear(render, width, height);

			onCompleted?.Invoke(render);
			
			//StartCoroutine(CaptureRoutine(width, height, onCompleted));
		}

		IEnumerator CaptureRoutine(int width, int height, System.Action<Texture2D> onCompleted)
		{
			float sw = 1f * width / wct.width;
			float sh = 1f * height / wct.height;
			
			Texture2D render =  new Texture2D(width, height); 
			RenderTexture rt = RenderTexture.GetTemporary(width, height);

			Graphics.Blit(wct, rt, new Vector2(sw, sh), Vector2.zero); // Pass webcam (scaled) to render texture
			
			RenderTexture.active = rt;
			render.ReadPixels(new Rect(0, 0, width, height),0,0);
			render.Apply();

			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(rt);
			
			yield return new WaitForEndOfFrame();
			
			onCompleted?.Invoke(render);
		}

		Resolution FindLowestResolutionForDevice(WebCamDevice device)
		{
			Resolution[] resolutions = device.availableResolutions;
			if(resolutions == null || resolutions.Length <= 0) throw new SystemException("No available resolutions for device!");
			
			Resolution res = resolutions[0];
			
			int w = res.width;
			int h = res.height;

			for (int i = 0; i < resolutions.Length; i++) {
				var _res = resolutions[i];
				var _w = _res.width;
				var _h = res.height;
				
				Debug.LogWarning($"Discover {device.name} resolution: {_w}x{_h}");
				if (i > 0 && (_w < w || _h < h)) res = _res;
			}

			return res;
		}
	}
}