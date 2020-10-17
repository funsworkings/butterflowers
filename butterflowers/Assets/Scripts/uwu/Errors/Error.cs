using System;
using UnityEngine;
using uwu.Behaviors;

namespace uwu.Errors
{
	[Serializable]
	public class Error
	{
		public string message = "";

		[HideInInspector] public Exception exception;
		[HideInInspector] public IErrorHandler handler;

		#region Events

		public Action<Error> onDispose;

		#endregion

		public Error(Exception exception, IErrorHandler handler)
		{
			this.exception = exception;
			this.handler = handler;

			message = exception != null ? exception.Message : "";
		}

		#region Operations

		public void Dispose()
		{
			if (handler != null)
				handler.OnResolveError();

			if (onDispose != null)
				onDispose(this);
		}

		#endregion
	}
}