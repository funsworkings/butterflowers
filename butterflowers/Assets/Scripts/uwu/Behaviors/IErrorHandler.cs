using System;

namespace uwu.Behaviors
{
	public interface IErrorHandler
	{
		void OnReceiveError(Exception err);
		void OnResolveError();
	}
}