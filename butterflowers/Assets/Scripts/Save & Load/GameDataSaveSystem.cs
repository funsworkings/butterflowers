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

		public bool wizard
		{
			get => data == null ? false : data.wizard;
			set => data.wizard = true;
		}

		public string[] files
		{
			get { return brainData == null ? new string[] { } : brainData.files; }
			set => brainData.files = value;
		}

		public float time
		{
			get => data == null ? 0f : data.time;
			set => data.time = value;
		}

		public LogData logs
		{
			get => data == null ? new LogData() : data.logs;
			set => data.logs = value;
		}

		public Log[] log_entries
		{
			get { return data == null ? new Log[] { } : data.logs.logs; }
			set => data.logs.logs = value;
		}

		public int chapter
		{
			get => data.chapter;
			set => data.chapter = value;
		}

		public int nestcapacity
		{
			get => data == null ? 6 : data.nestcapacity;
			set => data.nestcapacity = value;
		}

		public int[] discovered
		{
			get
			{
				var discoveries = brainData == null ? null : brainData.discoveries;
				if (discoveries == null) {
					discoveries = new int[] { };
					brainData.discoveries = discoveries;
				}

				return discoveries;
			}
			set => brainData.discoveries = value;
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
					var parsed = new BeaconData(beacon.file, beacon.type, beacon.state);

					dat.Add(parsed);
				}

				data.beacons = dat.ToArray();
			}
		}

		public VineData[] vineData
		{
			get { return data == null ? new VineData[] { } : data.vines; }
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

				data.vines = dat.ToArray();
			}
		}

		public bool nestOpen
		{
			get => data == null ? false : data.nestopen;
			set => data.nestopen = value;
		}

		public int dialogueNode
		{
			get => data == null ? -1 : data.dialoguenode;
			set => data.dialoguenode = value;
		}

		public int[] dialogueVisited
		{
			get { return data == null ? new int[] { } : data.dialoguevisited; }
			set => data.dialoguevisited = value;
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

		public int[] shared_files
		{
			get { return brainData == null ? new int[] { } : brainData.shared_files; }
			set => brainData.shared_files = value;
		}

		#endregion
	}
}