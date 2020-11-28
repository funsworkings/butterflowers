using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace uwu.Data
{
	public static class DataHandler
	{
		public static bool ready = true;

		public static bool Write(object dat, string path)
		{
			var bf = new BinaryFormatter();
			var file = File.Open(path, FileMode.Create);

			try {
				bf.Serialize(file, dat);
				file.Close();

				return true;
			}
			catch (Exception e) {
				file.Close();
				return false;
			}
		}

		public static T Read<T>(string path)
		{
			if (File.Exists(path)) 
			{
				var bf = new BinaryFormatter();
				var file = File.Open(path, FileMode.Open);

				var obj = bf.Deserialize(file);
				file.Close();

				try {
					T dat = (T) obj;
					return dat;
				}
				catch (System.Exception err) 
				{
					return default;
				}
			}

			return default;
		}

		public static T ReadJSON<T>(string path)
		{
			if (File.Exists(path)) {
				var json = File.ReadAllText(path);

				var dat = JsonUtility.FromJson<T>(json);
				return dat;
			}

			return default;
		}

		public static void WriteJSON<T>(T dat, string path, bool pretty)
		{
			var json = JsonUtility.ToJson(dat, pretty);
			File.WriteAllText(path, json);
		}

		public static bool Delete(string path)
		{
			if (File.Exists(path)) {
				File.Delete(path);

				return true;
			}

			return false;
		}
	}
}