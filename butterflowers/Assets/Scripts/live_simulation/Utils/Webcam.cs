using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using uwu.Textures;

namespace live_simulation.Utils
{
    public class Webcam : MonoBehaviour
	{
		// Properties
		
		[SerializeField] string _defaultDeviceId;

		private List<WebCamDevice> AvailableDevices = new List<WebCamDevice>();
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
			}

			if (!string.IsNullOrEmpty(_defaultDeviceId))
			{
				RequestDevice(_defaultDeviceId, success =>
				{
					Debug.LogWarning($"Auto-start for webcam was successful? {success}");
				});
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
		}

		public void RequestDevice(string deviceId, Action<bool> onReady)
		{
			WebCamTexture texture = SetupDevice(deviceId);
			if (texture != null)
			{
				DestroyDevice(); // Clear out previous device
				wct = texture; // Assign new wsebcam texture
				
				UpdateActiveDeviceIndex();
				onReady?.Invoke(true);
			}
			else
			{
				onReady?.Invoke(false);
			}
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

		public void RequestPreviousDevice(Action<bool> onReady)
		{
			if (DeviceIndex >= 0)
			{
				var index = Mathf.FloorToInt(Mathf.Repeat(DeviceIndex - 1, AvailableDevices.Count-1));
				RequestDeviceByIndex(index, onReady);
			}
			else
			{
				onReady?.Invoke(false);
			}
		}

		public void RequestNextDevice(Action<bool> onReady)
		{
			if (DeviceIndex >= 0)
			{
				var index = Mathf.FloorToInt(Mathf.Repeat(DeviceIndex + 1, AvailableDevices.Count));
				RequestDeviceByIndex(index, onReady);
			}
			else
			{
				onReady?.Invoke(false);
			}
		}

		void RequestDeviceByIndex(int index, Action<bool> onReady)
		{
			if (index != DeviceIndex && index >= 0 && index < AvailableDevices.Count)
			{
				var device = AvailableDevices[index];
				RequestDevice(device.name, onReady);
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