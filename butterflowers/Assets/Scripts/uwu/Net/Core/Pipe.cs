using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace uwu.Net.Core
{
	public sealed class Pipe : MonoBehaviour
	{
		public bool debug;


		#region Helpers

		public Exception ParseRequestForError(UnityWebRequest request, string response)
		{
			Exception error = null;

			if (request.isNetworkError) {
				error = new Exception("Network error*Unable to connect to server");
			}
			else {
				var status = Mathf.FloorToInt(request.responseCode);

				if (status > 199 && status < 300) {
					error = null;
				}
				else {
					var message = "Unknown error*?????";

					try {
						var err = JsonUtility.FromJson<Error>(response);
						message = err.message;
					}
					catch (Exception e) 
					{
						message = string.Format("Server error*{0} message*{1}", status, e.Message);
					}

					error = new Exception(message);
					Debug.Log("status: " + status);
				}
			}

			return error;
		}

		#endregion

		#region Internal

		public class Error
		{
			public string message = "";
		}

		#endregion

		#region Callbacks

		public PipeEvent GET_callback = new PipeEvent();
		public PipeEvent POST_callback = new PipeEvent();
		public PipeEvent PUT_callback = new PipeEvent();
		public PipeEvent DELETE_callback = new PipeEvent();

		#endregion


		#region Entry web operations

		public void GET<E>(string uri)
		{
			StartCoroutine(Get<E>(uri));
		}

		public void POST<E>(string uri, object dat)
		{
			var json = JsonUtility.ToJson(dat);
			StartCoroutine(Post<E>(uri, json));
		}

		public void PUT<E>(string uri, object dat)
		{
			var json = JsonUtility.ToJson(dat);
			StartCoroutine(Put(uri, json));
		}

		public void DELETE(string uri)
		{
			StartCoroutine(Delete(uri));
		}

		#endregion

		#region Coroutine web operations

		IEnumerator Get<E>(string uri)
		{
			var req = UnityWebRequest.Get(uri);

			req.SendWebRequest();
			while (!req.isDone)
				yield return null;

			E dat = default;

			var ret = Encoding.UTF8.GetString(req.downloadHandler.data);
			var err = ParseRequestForError(req, ret);

			try {
				dat = JsonUtility.FromJson<E>(ret);
			}
			catch (Exception e) {
				Debug.LogErrorFormat("uri: {0} err: {1}", uri, e.Message);
			}

			if (debug) Debug.LogFormat("GET {0} -> {1}", uri, ret);

			if (GET_callback != null)
				GET_callback.Invoke(err, dat);

			req.Dispose();
		}

		IEnumerator Post<E>(string uri, string json)
		{
			var req = UnityWebRequest.Post(uri, json);

			if (!string.IsNullOrEmpty(json)) {
				req.SetRequestHeader("content-type", "application/json");
				req.uploadHandler.contentType = "application/json";
				req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
			}

			req.SendWebRequest();
			while (!req.isDone)
				yield return null;

			E dat = default;

			var ret = Encoding.UTF8.GetString(req.downloadHandler.data);
			var err = ParseRequestForError(req, ret);

			try {
				dat = JsonUtility.FromJson<E>(ret);
			}
			catch (Exception e) {
				err = e;
				dat = default;
			}

			if (debug) Debug.LogFormat("POST {0} -> {1}", uri, ret);

			if (POST_callback != null)
				POST_callback.Invoke(err, dat);

			req.Dispose();
		}

		IEnumerator Put(string uri, string json)
		{
			var req = UnityWebRequest.Put(uri, json);

			if (!string.IsNullOrEmpty(json)) {
				req.SetRequestHeader("content-type", "application/json");
				req.uploadHandler.contentType = "application/json";
				req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
			}

			req.SendWebRequest();
			while (!req.isDone)
				yield return null;

			var ret = Encoding.UTF8.GetString(req.downloadHandler.data);
			var err = ParseRequestForError(req, ret);

			if (debug) Debug.LogFormat("PUT {0} -> {1}", uri, ret);

			if (PUT_callback != null)
				PUT_callback.Invoke(err, null);

			req.Dispose();
		}

		IEnumerator Delete(string uri)
		{
			var req = UnityWebRequest.Delete(uri);
			req.downloadHandler = new DownloadHandlerBuffer();

			req.SendWebRequest();
			while (!req.isDone)
				yield return null;

			var ret = Encoding.UTF8.GetString(req.downloadHandler.data);
			var err = ParseRequestForError(req, ret);

			if (debug) Debug.LogFormat("DELETE {0} -> {1}", uri, ret);

			if (DELETE_callback != null)
				DELETE_callback.Invoke(err, null);

			req.Dispose();
		}

		#endregion
	}
}