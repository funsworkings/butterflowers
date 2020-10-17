using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using uwu.Data;
using uwu.Extensions;
using uwu.IO;
using Wizard;

namespace uwu
{
	public partial class GameDataSaveSystem : Singleton<GameDataSaveSystem>
	{
		const string savefile = "save.dat";
		const float refreshrate = 15f;
		public static Action<bool> onLoad;


		[SerializeField] GameData m_data;

		bool m_autosave;

		public bool autosave
		{
			get => m_autosave;
			set
			{
				if (autosave != value) {
					if (!value) StopCoroutine("Autosave");
					else StartCoroutine("Autosave");
				}

				m_autosave = value;
			}
		}

		public GameData data => m_data;

		public bool load { get; set; }

		void OnEnable()
		{
			LoadGameData();
		}

		void OnDisable()
		{
			SaveGameData();
			autosave = false;
		}

		void OnApplicationPause(bool pause)
		{
			if (pause) {
				SaveGameData();
				autosave = false;
			}
			else {
				LoadGameData();
			}
		}

		IEnumerator Autosave()
		{
			while (true) {
				yield return new WaitForSeconds(refreshrate);
				SaveGameData();
			}
		}

		void SaveGameData()
		{
			if (data == null) {
				Debug.LogWarning("Attempted to save when no data available!");
				return;
			}

			onSaveGameData();

			var version = Application.version;
			var timestamp = string.Format("{0} - {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());

			data.BUILD_VERSION = version;
			data.TIMESTAMP = timestamp;

			DataHandler.Write(m_data, dataPath);
		}

		public void ResetGameData()
		{
			m_data = new GameData();
			SaveGameData();
		}

		public void LoadGameData(bool events = false, bool create = true)
		{
			var previous = false;

			var load = DataHandler.Read<GameData>(dataPath);
			if (load == null) {
				Debug.LogWarning("No save file located, initializing data file...");
				if (create) m_data = new GameData();
			}
			else {
				m_data = load;
				previous = true;
			}

			onLoadGameData();
			this.load = true;

			if (events)
				if (onLoad != null)
					onLoad(previous);
		}

		#region Fetch data path

		string m_dataPath;

		public string dataPath
		{
			get
			{
				var dir = Application.persistentDataPath + DataPaths.DATA_PATH;
				FileUtils.EnsureDirectory(dir);

				if (m_dataPath == null)
					m_dataPath = Path.Combine(dir, savefile);

				Debug.LogFormat("Save data path = {0}", m_dataPath);
				return m_dataPath;
			}
		}

		#endregion

		#region Save/load callbacks

		void onSaveGameData()
		{
			Debug.LogFormat("~~Save file was SAVED on {0}~~", DateTime.Now.ToShortTimeString());
		}

		void onLoadGameData()
		{
			Debug.LogFormat("~~Save file was LOADED on {0}~~", DateTime.Now.ToShortTimeString());
			autosave = refreshrate > 0f;
		}

		#endregion
	}
}