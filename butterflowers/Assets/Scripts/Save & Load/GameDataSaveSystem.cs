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
using AI.Types;
using Data;

namespace uwu
{
	public partial class GameDataSaveSystem : Singleton<GameDataSaveSystem>
	{
		[SerializeField] SceneData sceneDat;
		[SerializeField] BrainData brainDat;

		public SceneData data
		{
			get
			{
				var dat = (SceneData)cache_data_lookup["save.dat"];
				sceneDat = dat;

				return dat;
			}
		}

		public BrainData brainData
		{
			get
			{
				var dat =  (BrainData) cache_data_lookup["brain.fns"];
				brainDat = dat;

				return dat;
			}
		}
		
		public bool load => data_lookup.ContainsKey(def_savefile);
		
		#region External access

		public string username
		{
			get => (brainData == null) ? null : brainData.username;
			set => brainData.username = value;
		}

		public string[] directories
		{
			get { return brainData == null ? new string[] { } : brainData.directories; }
			set => brainData.directories = value;
		}

		public string[] files
		{
			get { return brainData == null ? new string[] { } : brainData.files; }
			set => brainData.files = value;
		}
		
		public int[] user_files
		{
			get { return brainData == null ? new int[] { } : brainData.user_files; }
			set => brainData.user_files = value;
		}
		
		public int[] shared_files
		{
			get { return brainData == null ? new int[] { } : brainData.shared_files; }
			set => brainData.shared_files = value;
		}
		
		public int[] world_files
		{
			get { return brainData == null ? new int[] { } : brainData.world_files; }
			set => brainData.world_files = value;
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

		public int chapter
		{
			get => data.chapter;
			set => data.chapter = value;
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
			get { return brainData == null ? new Knowledge[] { } : brainData.file_knowledge; }
			set => brainData.file_knowledge = value;
		}

		#endregion
	}
}