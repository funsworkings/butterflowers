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
				var dat = (GameData)cache_data_lookup["save.dat"];
				sceneDat = dat;

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

		public float enviro_knowledge
		{
			get => data == null ? 0f : data.enviro_knowledge;
			set => data.enviro_knowledge = value;
		}

		public bool IsProfileValid()
		{
			if (data == null) return false;

			var brainData = data.brain;
			return brainData.IsProfileValid();
		}

		#endregion
	}
}