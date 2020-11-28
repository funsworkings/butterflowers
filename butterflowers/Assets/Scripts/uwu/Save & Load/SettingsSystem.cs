using UnityEngine;
using uwu.Extensions;
using System;

namespace uwu
{
	public class SettingsSystem : Singleton<SettingsSystem>
	{
		
		#region Common access
		
		public char GetChar(string key, char @default) { return (char)GetValue<char>(key, @default); }
		public void SetChar(string key, char value) { SetValue<char>(key, value); }
		
		public string GetString(string key, string @default) { return (string)GetValue<string>(key, @default); }
		public void SetString(string key, string value) { SetValue<string>(key, value); }
		
		public bool GetBool(string key, bool @default) { return (bool)GetValue<bool>(key, @default); }
		public void SetBool(string key, bool value) { SetValue<bool>(key, value); }
		
		public int GetInt(string key, int @default) { return (int)GetValue<int>(key, @default); }
		public void SetInt(string key, int value) { SetValue<int>(key, value); }
		
		public float GetFloat(string key, float @default) { return (float)GetValue<float>(key, @default); }
		public void SetFloat(string key, float value) { SetValue<float>(key, value); }
		
		#endregion
		
		#region Functions

		public object GetValue<E>(string key, E def)
		{
			try {
				Type type;
				validateType(def, out type); // Validate type for setting (GET)

				if (PlayerPrefs.HasKey(key)) {
					if (type == typeof(int))
						return PlayerPrefs.GetInt(key);
					if (type == typeof(float))
						return PlayerPrefs.GetFloat(key);
					if (type == typeof(string))
						return PlayerPrefs.GetString(key);
					
					throw new System.Exception("Invalid type assigned for GET");
				}
				else 
				{
					SetValue<E>(key, def);
					return def; // Fallback to default value for entry
				}
			}
			catch (System.Exception e) { // Invalid type for value func
				Debug.LogErrorFormat("Failed to GET setting for : key=>{0} err=>{1}", key, e.Message);
				return null;
			}
		}

		public void SetValue<E>(string key, object value)
		{
			try {
				Type type;
				E @default = default(E);
				
				validateType(@default, out type); // Validate type for setting (PUT)

				if (type == typeof(int)) 
					PlayerPrefs.SetInt(key, (int)value);
				else if(type == typeof(float))
					PlayerPrefs.SetFloat(key, (float)value);
				else if(type == typeof(string))
					PlayerPrefs.SetString(key, (string) value);
			}
			catch (System.Exception e) { // Invalid type for value func
				Debug.LogErrorFormat("Failed to PUT setting for : key=>{0} value=>{1} err=>{2}", key, value, e.Message);
			}
		}
		
		#endregion
		
		
		#region Validate type

		private void validateType<E>(E @object, out Type cast_type)
		{
			var type = typeof(E);
			
			var int_type = typeof(int);
			var str_type = typeof(string);
			var float_type = typeof(float);
			
			if (type == typeof(bool) ||
			    type == int_type) {
				cast_type = int_type;
				return;
			}

			if (type == float_type) {
				cast_type = float_type;
				return;
			}

			if (type == typeof(char) || type == str_type) {
				cast_type = str_type;
				return;
			}
			
			throw new System.Exception("Unsupported type of settings op => " + type);
		}

		#endregion
		
	}
}