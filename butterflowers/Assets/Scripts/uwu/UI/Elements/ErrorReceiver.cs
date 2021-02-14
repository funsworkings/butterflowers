using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using uwu.Behaviors;
using uwu.Errors;

namespace uwu.UI.Elements
{
	public class ErrorReceiver : MonoBehaviour
	{
		public static ErrorReceiver Instance;

		#region Events

		public UnityEvent OnEnabled, OnDisabled;

		#endregion

		#region Properties

		[SerializeField] TMP_Text errorTitleField = null, errorBodyField = null;

		#endregion

		#region Attributes

		[SerializeField] int errorsInQueue;

		#endregion

		public void Push(string title, string body, IErrorHandler handler)
		{
			var message = string.Format("{0}*{1}", title, body);
			var exception = new Exception(message);

			Push(exception, handler);
		}

		public void Push(Exception exception, IErrorHandler handler)
		{
			var error = new Error(exception, handler);

			Push(error);
		}

		public void Push(Error error)
		{
			errors.Add(error); // Add to error queue

			if (errors.Count == 1) {
				OnEnabled.Invoke(); // Invoke enable to show error receiver
				errorInFocus = error; // Immediately pop first error in queue
			}
		}

		public void Pop()
		{
			errorInFocus = null;

			if (errors.Count > 0) {
				errors.RemoveAt(0);

				if (errors.Count > 0) {
					var error = errors[0];
					errorInFocus = error;
				}
				else {
					OnDisabled.Invoke(); // Invoke disable to hide error receiver
				}
			}
		}

		public void Clear()
		{
			if (errorInFocus == null)
				return;

			errorInFocus.Dispose();
			Pop();
			//errorInFocus = null;
		}

		void onDisposeError(Error error)
		{
			if (errors.Contains(error))
				errors.Remove(error); // Remove from error collection

			//       error.onDispose -= onDisposeError;
			if (errors.Count == 0)
				OnDisabled.Invoke(); // Invoke disable to hide error receiver
			else
				Pop(); // Immediately 
		}

		void DisplayError(Error error)
		{
			var message = error.exception != null ? error.exception.Message : "";
			var message_parts = message.Split('*');

			// Detected message with multiple astricks
			if (message_parts.Length > 2) {
				var message_title = message_parts[0];
				var message_body = "";

				var message_end = new string[message_parts.Length - 1];

				Array.Copy(message_parts, 1, message_end, 0, message_end.Length);
				message_body = string.Join("", message_end);

				message_parts = new string[2] {message_title, message_body};
			}

			if (message_parts.Length == 0)
				message_parts = new[] {""};

			var hasTitleAndBody = message_parts.Length == 2;

			if (errorTitleField != null) {
				errorTitleField.enabled = true;
				if (hasTitleAndBody)
					errorTitleField.text = message_parts[0];
				else
					errorTitleField.text = "Error";
			}

			if (errorBodyField != null) {
				errorBodyField.enabled = true;
				errorBodyField.text = hasTitleAndBody ? message_parts[1] : message_parts[0];
			}
		}

		#region Helpers

		bool HandlerInQueue(IErrorHandler handler)
		{
			var flag = false;
			for (var i = 0; i < errors.Count; i++)
				if (errors[i].handler == handler) {
					flag = true;
					break;
				}

			return flag;
		}

		#endregion

		#region Collections

		List<Error> errors = new List<Error>();
		Error errorInFocus;

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			if (Instance == null)
				Instance = this;
			else
				Destroy(gameObject); // Clear out Error receiver instance
		}

		void OnEnable()
		{
			errors = new List<Error>();
			errorInFocus = null;

			OnDisabled.Invoke();
		}

		void Update()
		{
			errorsInQueue = errors.Count;

			if (errorInFocus == null)
				return;

			DisplayError(errorInFocus);
		}

		void OnDestroy()
		{
			if (Instance == this) Instance = null; // Clear out previous instance
		}

		#endregion
	}
}