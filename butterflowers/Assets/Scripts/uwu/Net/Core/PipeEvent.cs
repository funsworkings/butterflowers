using System;
using UnityEngine.Events;

namespace uwu.Net.Core
{
	[Serializable]
	public sealed class PipeEvent : UnityEvent<Exception, object>
	{
	}
}