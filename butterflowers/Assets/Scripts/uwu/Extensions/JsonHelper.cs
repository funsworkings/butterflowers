using System;
using UnityEngine;

namespace uwu.Extensions
{
	public class JsonHelper
	{
		public static T[] getJsonArray<T>(string json)
		{
			var newJson = "{ \"array\": " + json + "}";
			var wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
			return wrapper.array;
		}

		[Serializable]
		class Wrapper<T>
		{
			public T[] array = new T[]{};
		}
	}
}