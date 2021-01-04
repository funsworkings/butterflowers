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
using Data;
using Neue.Agent.Brain.Data;
using Neue.Types;

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

		public bool load => data_lookup.ContainsKey(def_savefile);
		
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

		public Knowledge[] file_knowledge
		{
			get { return data == null ? new Knowledge[] { } : data.file_knowledge; }
			set => data.file_knowledge = value;
		}

		#endregion
	}
}