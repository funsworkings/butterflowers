using System;
using UnityEngine;
using uwu.Extensions;

namespace uwu.Settings
{
	public abstract class Controller : MonoBehaviour
	{
		public enum KeyType
		{
			Float, String, Integer, Char, Boolean
		}
		
		public void ApplySetting(string key, object value, KeyType type)
		{
			switch (type) 
			{
				case KeyType.Float:
					PlayerPrefs.SetFloat(key, (float)value);
					break;
				case KeyType.Boolean:
					bool bVal = (bool) value;
					value = (int) ((bVal) ? 1 : 0);
					PlayerPrefs.SetInt(key, (int)value);
					break;
				case KeyType.Integer:
					PlayerPrefs.SetInt(key, (int)value);
					break;
				case KeyType.Char:
					char cVal = (char) value;
					value = cVal.ToString(); 
					PlayerPrefs.SetString(key, (string)value);
					break;
				case KeyType.String:
					PlayerPrefs.SetString(key, (string) value);
					break;
			}	
		}

		public object FetchSetting(string key, KeyType type)
		{
			switch (type) 
			{
				case KeyType.Float:
					return PlayerPrefs.GetFloat(key);
				case KeyType.Boolean:
					int bVal = PlayerPrefs.GetInt(key);
					return (bVal == 1);
				case KeyType.Integer:
					return PlayerPrefs.GetInt(key);
				case KeyType.Char:
					string cVal = PlayerPrefs.GetString(key);
					return (cVal[0]);
				case KeyType.String:
					return PlayerPrefs.GetString(key);
			}
			
			throw new SystemException("Undefined key type for settings!");
		}
	}
}