using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using uwu.Textures;
using Random = UnityEngine.Random;

namespace live_simulation.Utils
{
    public class Webcam : MonoBehaviour
	{
		// Properties

		[SerializeField] private Camera _renderCamera;
		[SerializeField] private List<GameObject> toggleVisibilityComponents = new List<GameObject>();
		[SerializeField] private bool _autoStart = true;

		private string _current;
		private List<WebCamDevice> AvailableDevices = new List<WebCamDevice>();

		private List<string> _used = new List<string>();
		private List<string> _cache = new List<string>();
		
		private int DeviceIndex = -1;
		
		WebCamTexture wct;
		public WebCamTexture CurrentActiveRenderTarget => wct;

		public bool Ready => wct != null && wct.isPlaying;
		
		void Start()
		{
			WebCamDevice? t_device = null;
			
			var devices = WebCamTexture.devices;
			foreach (WebCamDevice device in devices)
			{
				string deviceId = device.name;
				Debug.Log($"Discovered video device: {deviceId}");

				AvailableDevices.Add(device);	
				_cache.Add(deviceId);
			}

			if (_autoStart && AvailableDevices.Count > 0)
			{
				RequestDevice(AvailableDevices[0].name, texture =>
				{
					Debug.LogWarning($"Auto-start for webcam was successful? {texture.deviceName}");
				});
			}
		}

		public void Capture(int width, int height, System.Action<Texture2D> onCompleted, int captureWidth = 0, int captureHeight = 0, int captureOX = 0, int captureOY = 0)
		{
			if(!Ready) throw new System.Exception("Device not ready!");

			var w = _renderCamera.pixelWidth;
			var h = _renderCamera.pixelHeight;
			
			Rect _render = new Rect(0,0, w, h);

			captureOY = h - captureOY;

			if (captureWidth > 0 || captureHeight > 0)
			{
				captureWidth = Mathf.Clamp(captureWidth, 1, w);
				captureHeight = Mathf.Clamp(captureHeight, 1, h);

				captureOX = Mathf.Clamp(captureOX, 0, w - captureWidth);
				captureOY = Mathf.Clamp(captureOY, 0, h - captureHeight);

				_render = new Rect(captureOX, captureOY, captureWidth, captureHeight);
			}

			StartCoroutine(CaptureRoutine(w, h, _render, onCompleted));
		}

		IEnumerator CaptureRoutine(int w, int h, Rect captureRegion, Action<Texture2D> callback)
		{
			ToggleComponents(false);
			
			yield return new WaitForEndOfFrame();
			
			Texture2D render =  new Texture2D((int)captureRegion.width, (int)captureRegion.height, TextureFormat.RGB24, false); 
			//Debug.LogWarning($"Render-- x:{_render.x} y:{_render.y} w:{_render.width} h:{_render.height} {render.width} {render.height} {w} {h}");
			
			RenderTexture renderTexture = new RenderTexture(w, h, 24);

			_renderCamera.targetTexture = renderTexture;
			_renderCamera.Render();
 
			RenderTexture.active = renderTexture;
			render.ReadPixels(captureRegion, 0, 0);
			render.Apply();
 
			_renderCamera.targetTexture = null;
			RenderTexture.active = null;
 
			Destroy(renderTexture);
			renderTexture = null;

			//yield return new WaitForEndOfFrame();
			
			//TextureScale.Bilinear(render, width, height);
			
			ToggleComponents(true);
			callback?.Invoke(render);
		}

		void ToggleComponents(bool visible)
		{
			foreach(GameObject o in toggleVisibilityComponents) o.SetActive(visible);
		}

		public void RequestDevice(string deviceId, Action<WebCamTexture> onReady)
		{
			WebCamTexture texture = SetupDevice(deviceId);
			if (texture != null)
			{
				DestroyDevice(); // Clear out previous device
				
				wct = texture; // Assign new wsebcam texture
				_current = deviceId;
				
				if (_cache.Contains(deviceId))
				{
					_cache.Remove(deviceId);
					if (_cache.Count == 0)
					{
						// Reset collections
						_cache = new List<string>(_used);
						_used = new List<string>();
					}
				}
				if (!_used.Contains(deviceId)) _used.Add(deviceId);

				UpdateActiveDeviceIndex();
			}
			
			onReady?.Invoke(texture);
		}

		WebCamTexture SetupDevice(string deviceId)
		{
			try
			{
				var wct = new WebCamTexture(deviceId);
				wct.Play();

				return wct;
			}
			catch (SystemException e)
			{
				Debug.LogError(e);
				return null;
			}
		}

		void DestroyDevice()
		{
			if (wct != null)
			{
				wct.Stop();
				DestroyImmediate(wct);

				wct = null;
				
				UpdateActiveDeviceIndex();
			}
		}

		void UpdateActiveDeviceIndex()
		{
			if (wct != null)
			{
				string deviceId = wct.deviceName;
				for (int i = 0; i < AvailableDevices.Count; i++)
				{
					if (AvailableDevices[i].name == deviceId)
					{
						DeviceIndex = i;
						return;
					}
				}
			}
			
			DeviceIndex = -1;
		}
		
		#region Sequence ops

		public void RequestPreviousDevice(Action<WebCamTexture> onReady)
		{
			if (DeviceIndex >= 0)
			{
				var index = Mathf.FloorToInt(Mathf.Repeat(DeviceIndex - 1, AvailableDevices.Count-1));
				RequestDeviceByIndex(index, onReady);
			}
			else
			{
				onReady?.Invoke(wct);
			}
		}

		public void RequestNextDevice(Action<WebCamTexture> onReady)
		{
			if (DeviceIndex >= 0)
			{
				var index = Mathf.FloorToInt(Mathf.Repeat(DeviceIndex + 1, AvailableDevices.Count));
				RequestDeviceByIndex(index, onReady);
			}
			else
			{
				onReady?.Invoke(wct);
			}
		}

		public void RequestRandomDevice(Action<WebCamTexture> onReady)
		{
			var _item = _cache[Random.Range(0, _cache.Count)];
			RequestDevice(_item, onReady);
		}

		void RequestDeviceByIndex(int index, Action<WebCamTexture> onReady)
		{
			if (index != DeviceIndex && index >= 0 && index < AvailableDevices.Count)
			{
				var device = AvailableDevices[index];
				RequestDevice(device.name, onReady);
			}
			else
			{
				onReady?.Invoke(wct);
			}
		}
		
		#endregion

		/*
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

		Resolution RequestResolutionForWebcamDevice(WebCamDevice device, float quality = 0f)
		{
			quality = Mathf.Clamp01(quality); // Ensure 0-1 quality setting
			
			Resolution[] resolutions = device.availableResolutions;
			if(resolutions == null || resolutions.Length <= 0) throw new SystemException("No available resolutions for device!");
			
			Resolution res = resolutions[0];
			
			int w = res.width;
			int h = res.height;

			for (int i = 0; i < resolutions.Length; i++) 
			{
				var _res = resolutions[i];
				var _w = _res.width;
				var _h = res.height;
				
				Debug.LogWarning($"Discover {device.name} resolution: {_w}x{_h}");
				if (i > 0 && (_w < w || _h < h)) res = _res;
			}

			return res;
		}
		*/
	}
}