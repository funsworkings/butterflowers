using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using uwu.Data;
using uwu.Extensions;
using uwu.IO;
using Wizard;

namespace uwu
{
	public partial class GameDataSaveSystem : Singleton<GameDataSaveSystem>
	{
		// Defaults
			const string def_savefile = "save.dat";
			const float def_refreshrate = 15f;

			// Collections
			protected Dictionary<string, string> file_lookup = new Dictionary<string, string>();
			protected Dictionary<string, object> data_lookup = new Dictionary<string, object>();
			protected Dictionary<string, object> cache_data_lookup = new Dictionary<string, object>();
			protected Dictionary<string, Type> type_lookup = new Dictionary<string, Type>();
			
			protected List<object> datas = new List<object>();

		
		
		#region Autosaving
		
		bool m_autosave;
		public bool autosave
		{
			get => m_autosave;
			set
			{
				if (autosave != value) 
				{
					if (!value) StopCoroutine("Autosave");
					else StartCoroutine("Autosave");
				}

				m_autosave = value;
			}
		}
		
		IEnumerator Autosave()
		{
			while (true) 
			{
				yield return new WaitForSeconds(def_refreshrate);
				SaveAllGameData();
			}
		}
		
		#endregion

		#region Save & load
		
		public bool HasData(string file)
		{
			return data_lookup.ContainsKey(file);
		}

		public bool SaveGameData(string file = null, bool useCacheSave = false)
		{
			if (file == null) file = def_savefile;

			object o = null;
			data_lookup.TryGetValue(file, out o);

			if (o == null) 
			{
				Debug.LogWarning("Attempted to save when no data available!");
				return false;
			}

			GameData cacheDat = (GameData) o;

			var version = Application.version;
			var timestamp = string.Format("{0} - {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());

			// Rebind data from list
			if (!useCacheSave) 
			{
				object d = cache_data_lookup[file];
				if (d != null) o = d;
			}

			(o as GameData).BUILD_VERSION = version;
			(o as GameData).TIMESTAMP = timestamp;

			data_lookup[file] = o; // Overwrite lookup
			onSaveGameData(file); // Callback!

			var path = file_lookup[file]; // Fetch rel file path
			return DataHandler.Write(o, path);
		}

		void SaveAllGameData()
		{
			KeyValuePair<string, object>[] dat = data_lookup.ToArray();
			foreach (KeyValuePair<string, object> d in dat) 
			{
				var file = d.Key;
				SaveGameData(file);
			}
		}

		public bool LoadGameData<E>(string file = null, bool createIfEmpty = false) where E:GameData
		{
			if (file == null) file = def_savefile; // Fallback to default save file

			E dat = null;
			string path = null;

			if (file_lookup.ContainsKey(file)) // Fetch rel file path
				path = file_lookup[file];
			else
				path = ConstructDataPath(file);
			
			dat = DataHandler.Read<E>(path);
			if (dat == null && createIfEmpty) 
			{
				Debug.LogWarningFormat("No save file located, initializing data file... [{0}]", file);
				dat = (E) Activator.CreateInstance(typeof(E));
			}
			
			bool success = (dat != null);
			if (success) 
			{
				Debug.LogFormat("Object success intiialize... [{0}]", file);
				PushGameData<E>(file, path, dat); // Push save file onto stack
			}

			onLoadGameData(file, success); // Callback!
			
			return success;
		}

		public void UnloadGameData(string file = null)
		{
			SaveGameData(file);
			PopGameData(file);
		}
		
		public void WipeGameData<E>(string file = null) where E:GameData
		{
			if (file == null) file = def_savefile;

			if (data_lookup.ContainsKey(file)) 
			{
				cache_data_lookup[file] = data_lookup[file] = (E)Activator.CreateInstance(typeof(E));
				SaveGameData(file, useCacheSave:true); // Overwrite existing game data for file
			}
		}

		public void EraseGameData(string file = null)
		{
			if (file == null) file = def_savefile;

			string path = null;
			if (file_lookup.TryGetValue(file, out path)) 
			{
				System.IO.File.Delete(path); // Erase file at path
				PopGameData(file);
			}
		}

		#endregion
		
		#region Push/pop
		
		void PushGameData<E>(string file, string path, E dat, string directory = null) where E:GameData
		{
			if (!file_lookup.ContainsKey(file)) 
			{
				file_lookup.Add(file, path);
			}
			
			if (!data_lookup.ContainsKey(file)) 
			{
				string id = dat.ID;
				print(id);
				
				data_lookup.Add(file, dat); // Add game data to lookup
			}

			if (!cache_data_lookup.ContainsKey(file)) 
			{
				cache_data_lookup.Add(file, dat);
			}

			if (!type_lookup.ContainsKey(file)) 
			{
				type_lookup.Add(file, dat.GetType());
			}
		}

		void PopGameData(string file)
		{
			if (file_lookup.ContainsKey(file))
				file_lookup.Remove(file);
			if (data_lookup.ContainsKey(file))
				data_lookup.Remove(file);
			if (cache_data_lookup.ContainsKey(file))
				cache_data_lookup.Remove(file);
			if (type_lookup.ContainsKey(file))
				type_lookup.Remove(file);
		}
		
		#endregion
		
		#region Query

		public string[] FindAllSaveFiles(string[] extensions = null, string directory = null)
		{
			if (directory == null) directory = Application.persistentDataPath;
			if(extensions == null) extensions = new string[]{ "dat" };
			
			if (System.IO.Directory.Exists(directory)) 
			{
				List<string> valid = new List<string>();
				
				var files = new DirectoryInfo(directory).GetFileSystemInfos();
				foreach (FileSystemInfo i in files) 
				{
					if (extensions.Contains(i.Extension)) 
					{
						valid.Add(i.Name);
					}
				}

				return valid.ToArray();
			}
			else 
			{
				throw new System.Exception("Directory for locating save files does not exist!");	
			}
		}
		
		#endregion

		#region Data path

		/// <summary>
		/// Constructs the data path for a save file - OPTIONAL: directory
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="ext"></param>
		/// <param name="directory"></param>
		public string ConstructDataPath(string filename, string directory = null)
		{
			if (directory == null)
				directory = Application.persistentDataPath;
			
			FileUtils.EnsureDirectory(directory);
			
			var path = Path.Combine(directory, filename);
			Debug.LogFormat("Save data path = {0}", path);
			
			return path;
		}

		#endregion

		#region Save/load callbacks

		void onSaveGameData(string file)
		{
			Debug.LogFormat("~~Save file [{0}] was SAVED on {1}~~", file, DateTime.Now.ToShortTimeString());
		}

		void onLoadGameData(string file, bool success)
		{
			var timestamp = DateTime.Now.ToShortTimeString();
			
			if(success)
				Debug.LogFormat("~~Save file [{0}] was LOADED on {1}~~", file, timestamp);
			else
				Debug.LogWarningFormat("Save file [{0}] FAILED TO LOAD on {1}", file, timestamp);
		}

		#endregion
	}
}