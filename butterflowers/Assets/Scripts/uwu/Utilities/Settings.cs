using System;
using System.Collections.Generic;
using UnityEngine;
using uwu.Extensions;

namespace uwu.Utilities
{
	public abstract class Settings : MonoBehaviour
	{
		#region Internal

		public enum Type
		{
			Float,
			String,
			Integer
		}

		#endregion
		
		/// <summary>
		/// Structured as Key => Type => Default Value
		/// </summary>
		protected abstract object[] Items
		{
			get;
		}


		protected virtual void Awake()
		{
			BindSettings();	
		}
		
		#region Binding

		void BindSettings()
		{
			var items = Items;
			if (items.Length % 3 != 0) 
			{
				throw new SystemException("Invalid parameter set for settings!");	
			}

			for (int i = 0; i < items.Length; i += 3) 
			{
				try 
				{
					string key = (string)items[ i ];
					Type type = (Type) items[ i + 1 ];
					object defaultValue = items[ i + 2 ];

					try 
					{
						object value = FetchSetting(key, type); // Check to see if setting has been applied before
						ApplySetting(key, type, value);
					}
					catch (SystemException err) 
					{
						ApplySetting(key, type, defaultValue, force:true);	
					}
				}
				catch (System.Exception err) 
				{
					Debug.LogWarning("Unable to parse setting item based on type!");	
				}
			}
		}
		
		#endregion
		
		#region Player prefs

		public void ApplySetting(string key, Type type, object value, bool force = false)
		{
			bool success = false;
			
			if (PlayerPrefs.HasKey(key) || force) 
			{
				switch (type) 
				{
					case Type.Float:
						PlayerPrefs.SetFloat(key, (float)value);
						success = true;
						break;
					case Type.Integer:
						PlayerPrefs.SetInt(key, (int)value);
						success = true;
						break;
					case Type.String:
						PlayerPrefs.SetString(key, (string)value);
						success = true;
						break;
				}
			}
			
			if(!success) Debug.LogWarning("Unable to find setting with key => " + key);
			else DidApplySetting(key, type, value);
		}

		public object FetchSetting(string key, Type type)
		{
			if (PlayerPrefs.HasKey(key)) 
			{
				switch (type) 
				{
					case Type.Float:
						return PlayerPrefs.GetFloat(key);
					case Type.Integer:
						return PlayerPrefs.GetInt(key);
					case Type.String:
						return PlayerPrefs.GetString(key);
				}
			}

			throw new SystemException("Unable to find setting with key => " + key);
		}
		
		#endregion
		
		#region Abstract impl

		protected abstract void DidApplySetting(string key, Type type, object val);

		#endregion
	}
}