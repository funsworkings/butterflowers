﻿using System;
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
			bool success = false;
			
			try 
			{
				var bf = new BinaryFormatter();
				var file = File.Open(path, FileMode.Create);

				// Passed open check!
				try 
				{
					bf.Serialize(file, dat);
					success = true;
				}
				catch (Exception e) 
				{
					Debug.LogWarning(e.Message);
				}
				
				file.Close();
			}
			catch (SystemException err) 
			{
				Debug.LogWarning(err.Message);
			}

			return success;
		}

		public static T Read<T>(string path)
		{
			T ret = default(T);
			
			if (File.Exists(path)) 
			{
				try 
				{
					var bf = new BinaryFormatter();
					var file = File.Open(path, FileMode.Open);

					try 
					{
						var obj = bf.Deserialize(file);
						T dat = (T) obj;

						ret = dat;
					}
					catch (System.Exception err) 
					{
						Debug.LogWarning(err.Message);
						return default;
					}
					
					file.Close();
				}
				catch (System.Exception err) 
				{
					Debug.LogWarning(err.Message);
					return default;
				}
			}

			return ret;
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