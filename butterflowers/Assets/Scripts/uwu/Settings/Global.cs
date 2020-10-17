using UnityEngine;

namespace uwu.Settings
{
	public class Global<U, T> : ScriptableObject where U : Setting<T>
	{
		public U[] settings;

		public T GetValueFromKey(string key)
		{
			foreach (var s in settings)
				if (s.key == key)
					return s.value;
			return default;
		}
	}
}