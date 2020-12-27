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

		public string[] directories
		{
			get { return data == null ? new string[] { } : data.directories; }
			set => data.directories = value;
		}

		public string[] files
		{
			get { return data == null ? new string[] { } : data.files; }
			set => data.files = value;
		}
		
		public int[] user_files
		{
			get { return data == null ? new int[] { } : data.user_files; }
			set => data.user_files = value;
		}
		
		public int[] shared_files
		{
			get { return data == null ? new int[] { } : data.shared_files; }
			set => data.shared_files = value;
		}
		
		public int[] world_files
		{
			get { return data == null ? new int[] { } : data.world_files; }
			set => data.world_files = value;
		}

		public SunData sun
		{
			get => data == null ? new SunData() : data.sun;
			set => data.sun = value;
		}

		public float time
		{
			get => data == null ? 0f : data.sun.time;
			set => data.sun.time = value;
		}

		public BeaconData[] beaconData
		{
			get { return data == null ? new BeaconData[] { } : data.beacons; }
		}

		public Beacon[] beacons
		{
			set
			{
				var dat = new List<BeaconData>();
				for (var i = 0; i < value.Length; i++) {
					var beacon = value[i];
					var parsed = new BeaconData(beacon.file, beacon.Origin, beacon.type, beacon.state);

					dat.Add(parsed);
				}

				data.beacons = dat.ToArray();
			}
		}

		public VineData[] vineData
		{
			get { return data == null ? new VineData[] { } : data.vines.vines; }
		}

		public Vine[] vines
		{
			set
			{
				var dat = new List<VineData>();

				for (var i = 0; i < value.Length; i++) {
					var vine = value[i];
					var parsed = new VineData(vine.state, vine.index, vine.interval, vine.height, vine.Waypoints, vine.file, vine.Leaves);

					dat.Add(parsed);
				}

				data.vines.vines = dat.ToArray();
			}
		}

		public bool nestOpen
		{
			get => data == null ? false : data.nestopen;
			set => data.nestopen = value;
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