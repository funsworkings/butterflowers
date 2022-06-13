using System;
using System.Globalization;
using butterflowersOS;
using Neue.Agent.Brain.Data;
using UnityEngine;
using uwu.Extensions;

namespace uwu
{
	public partial class GameDataSaveSystem : Singleton<GameDataSaveSystem>
	{
		[SerializeField] GameData sceneDat;
		[SerializeField] BrainData brainDat;

		public GameData data
		{
			get
			{
				GameData dat = null;
				if (cache_data_lookup.ContainsKey("save.dat")) 
				{
					dat = (GameData) cache_data_lookup["save.dat"];
					sceneDat = dat;
				}

				return dat;
			}
		}

		public bool load => data_lookup.ContainsKey(uwu.GameDataSaveSystem.def_savefile);
		
		#region External access

		public string username
		{
			get => (data == null) ? null : data.username;
			set => data.username = value;
		}
		
		public bool IsSelfProfileValid()
		{
			if (data == null) return false;
			return BrainDataExtensions.IsProfileTimestampValid(data.export_agent_created_at);
		}

		public bool IsExternalProfileValid()
		{
			if (data == null) return false;
			return !string.IsNullOrEmpty(data.import_agent_created_at);
		}

		#endregion
	}
}