using UnityEngine;
using uwu.Extensions;

namespace uwu.Net.Core
{
	public class WebHandler : Singleton<WebHandler>
	{
		#region Prefixes

		const string sms_uri = "sms:";

		#endregion


		public void SendSMS(string recipient, string body = "")
		{
#if UNITY_IOS && !UNITY_EDITOR
            string url = sms_uri + recipient + "?&body=" + System.Uri.EscapeDataString(body);
            OpenURL(url);
#elif UNITY_EDITOR
			Debug.LogFormat("sending \"{0}\" to {1}", body, recipient);
#endif
		}

		public void GoTo(string url)
		{
			OpenURL(url);
		}

		void OpenURL(string url)
		{
			if (string.IsNullOrEmpty(url)) {
				Debug.LogWarning("Attempting to open URL that is null or empty...");
				return;
			}

			Application.OpenURL(url);
		}
	}
}