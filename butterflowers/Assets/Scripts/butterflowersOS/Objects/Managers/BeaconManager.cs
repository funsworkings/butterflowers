using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using butterflowersOS.Core;
using butterflowersOS.Data;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Presets;
using live_simulation;
using UnityEngine;
using UnityEngine.Events;
using uwu;
using uwu.Extensions;
using uwu.Gameplay;
using uwu.IO;
using uwu.IO.SimpleFileBrowser.Scripts;
using Type = butterflowersOS.Objects.Entities.Interactables.Beacon.Type;
using Locale = butterflowersOS.Objects.Entities.Interactables.Beacon.Locale;
using Object = System.Object;
using Random = UnityEngine.Random;
using Transition = butterflowersOS.Objects.Entities.Interactables.Beacon.Transition;

namespace butterflowersOS.Objects.Managers
{
	public class BeaconManager : Spawner, IReactToSunCycle, ISaveable
	{
		public static BeaconManager Instance;

		#region Events

		public System.Action onUpdateBeacons;
		public System.Action<Beacon> onPlantBeacon;

		public UnityEvent onDestroyBeacon;

		#endregion

		#region External

		World World;
		Nest Nest;
		GameDataSaveSystem SaveSys;
		Library Library;
		FileNavigator Files;

		#endregion
    
		#region Internal

		public enum TransitionType
		{
			NULL,
	    
			Flower,
			Spawn
		}
    
		#endregion

		#region Properties

		[SerializeField] WorldPreset preset = null;
		[SerializeField] ObjectPool firePool = null, extinguishPool = null;
    
		public Transition flowerTransition;
		public Transition spawnTransition;

		[SerializeField] GameObject impactPS;

		#endregion

		#region Attributes

		[SerializeField] LayerMask beaconGroundMask;
		[SerializeField] int maxBeacons = 100;

		#endregion

		#region Collections

		Dictionary<string, List<Beacon>> beacons = new Dictionary<string, List<Beacon>>();
		List<Beacon> allBeacons = new List<Beacon>();

		#endregion

		#region Accessors

		public Beacon[] AllBeacons => allBeacons.ToArray();

		public Beacon[] ActiveBeacons => allBeacons.Where(beacon => Nest.HasBeacon(beacon)).ToArray();
		public Beacon[] InactiveBeacons => allBeacons.Where(beacon => !Nest.HasBeacon(beacon)).ToArray();
		
		public Beacon[] NestBeacons => allBeacons.Where(beacon => beacon.state == Locale.Nest).ToArray();
		public Beacon[] TerrainBeacons => allBeacons.Where(beacon => beacon.state == Locale.Terrain).ToArray();
		public Beacon[] FlowerBeacons => allBeacons.Where(beacon => beacon.state == Locale.Flower).ToArray();
		public Beacon[] PlantedBeacons => allBeacons.Where(beacon => beacon.state == Locale.Planted).ToArray();
		public Beacon[] LiveBeacons => allBeacons.Where(beacon => (beacon.state != Locale.Planted || beacon.state != Locale.Destroyed)).ToArray();

		public Beacon GetBeaconByFile(string file)
		{
			List<Beacon> b = null;
			beacons.TryGetValue(file, out b);

			if (b != null && b.Count > 0)
				return b[0];

			return null;
		}

		#endregion

		#region Monobehaviour callbacks

		protected override void Awake()
		{
			base.Awake();

			Instance = this;
		}

		protected override void Start()
		{
			base.Start();
			
			Beacon.OnRegister += onRegisterBeacon;
			Beacon.OnUnregister += onUnregisterBeacon;

			Beacon.Activated += onActivatedBeacon;
			Beacon.Deactivated += onDeactivatedBeacon;
			Beacon.Planted += onPlantedBeacon;
			Beacon.Flowered += onFloweredBeacon;
			Beacon.Destroyed += onDestroyedBeacon;
			Beacon.onFire += onFireBeacon;
			Beacon.onExtinguish += onExtinguishBeacon;
        
			Library = Library.Instance;
			Library.onDeletedFiles += UserDeletedFiles;
			Library.onRecoverFiles += UserRecoveredFiles;

			BridgeUtil.onCreateImage += CreateBeaconFromWebcamImage;
		}
		
		protected override void OnDestroy()
		{
			base.OnDestroy();
			
			Beacon.OnRegister -= onRegisterBeacon;
			Beacon.OnUnregister -= onUnregisterBeacon;

			Beacon.Activated -= onActivatedBeacon;
			Beacon.Deactivated -= onDeactivatedBeacon;
			Beacon.Planted -= onPlantedBeacon;
			Beacon.Flowered -= onFloweredBeacon;
			Beacon.Destroyed -= onDestroyedBeacon;
			Beacon.onFire -= onFireBeacon;
			Beacon.onExtinguish -= onExtinguishBeacon;

			if (Library.IsValid()) 
			{
				Library.onDeletedFiles -= UserDeletedFiles;
				Library.onRecoverFiles -= UserRecoveredFiles;
			}
			
			BridgeUtil.onCreateImage -= CreateBeaconFromWebcamImage;
		}

		protected override void Update()
		{

				if(Input.GetKeyDown(KeyCode.LeftBracket) && preset.allowDebugSpawn) DebugBeaconFromDesktop();

		}

		#endregion

		#region Cycle

		public void Cycle(bool refresh)
		{
			Debug.LogWarning($"Cycle beacons: {preset.clearBeaconsOnCycle}"); 
			if(preset.clearBeaconsOnCycle) WipeBeacons(); // Clear out flammable beacons
		}
    
		#endregion
		
		#region WV

		void CreateBeaconFromWebcamImage(string path)
		{
			return; // Ignore beacon manager calls to create beacon from webcam img
			
			Debug.Log($"Request beacon spawn: {path}");
			CreateBeacon(path, Type.Desktop, Locale.Terrain, new Hashtable(), transition:BeaconManager.TransitionType.Spawn);
		}
		
		#endregion

		#region Spawner overrides

		protected override void CalculateBounds()
		{
			m_center = Vector3.zero;
			m_extents = root.GetComponent<MeshFilter>().mesh.bounds.extents;
		}

		public override void DecidePosition(ref Vector3 pos)
		{
			base.DecidePosition(ref pos); // Get initial ray position

			Vector3 origin = pos + root.up * 9f;
			Vector3 dir = -root.up;

			Ray ray = new Ray(origin, dir);
			RaycastHit hit = new RaycastHit();

			if (Physics.Raycast(ray, out hit, 999f, beaconGroundMask.value)) 
			{
				pos = hit.point;
			}
		}

		public override void DecideRotation(ref Quaternion rot)
		{
			rot = transform.rotation;
		}

		#endregion

		#region Operations

		public Beacon CreateBeacon(string path, Type type, Locale state, Hashtable @params = null,
			TransitionType transition = TransitionType.NULL, bool fromSave = false)
		{
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			Vector3 origin = Vector3.zero;

			bool requirePosition = false;

			if (@params== null || !@params.ContainsKey("position")) { DecidePosition(ref position); requirePosition = true; }
			else position = (Vector3) @params["position"];

			if (@params== null || !@params.ContainsKey("origin")){ if(requirePosition) origin = position; else DecidePosition(ref origin);}
			else origin = (Vector3) @params["origin"];

			var beacon = InstantiatePrefab().GetComponent<Beacon>();
			return RegisterBeacon(beacon, path, type, state, position, rotation, origin, fromSave, transition);
		}

		Beacon RegisterBeacon(Beacon beacon, string path, Type type, Locale state, Vector3 position, Quaternion rotation, Vector3 origin, bool load, TransitionType transition)
		{
			var discovered = Library.HasDiscoveredFile(path); // Check if seen before
	    
			beacon.File = path;
			beacon.discovered = discovered; // Set if beacon has been discovered

			beacon.transform.position = position;
			beacon.transform.rotation = rotation;


			Transition _transition = null;
			if (transition == TransitionType.Flower) 
			{
				_transition = new Transition(flowerTransition) {
					posA = position,
					posB = origin,
					scaleA = Vector3.zero,
					scaleB = preset.normalBeaconScale * Vector3.one
				};
			}
			else if (transition == TransitionType.Spawn) 
			{
				_transition = new Transition(spawnTransition) {
					posA = origin, posB = origin, scaleA = Vector3.zero, scaleB = preset.normalBeaconScale * Vector3.one
				};
			}
			if(_transition != null) _transition.time = 0f; // Reset transition time

			
			beacon.Register(type, state, origin, _transition, load);
			
			var filetype = Library.FileType.World;
			if (beacon.type == Type.Desktop) filetype = Library.FileType.User;

			Library.RegisterFileInstance(path, filetype); // Register file with library
			Events.ReceiveEvent(EVENTCODE.BEACONADD, AGENT.World, AGENT.Beacon, details: beacon.File);

			Debug.LogWarning("Beacon was added = " + beacon.File);
	    
			return beacon;
		}

		public void DeleteBeacon(Beacon beacon, bool overridenest = false)
		{
			bool success = !(beacon.state == Locale.Planted || beacon.state == Locale.Flower);

			if (success) 
			{
				Events.ReceiveEvent(EVENTCODE.BEACONDELETE, AGENT.User, AGENT.Beacon, details: beacon.File);

				if (overridenest)
					beacon.Delete();
				else 
				{
					if (!Nest.HasBeacon(beacon)) // Don't delete if currently inside nest
						beacon.Delete();
					else
						success = false;
				}
			}
		}

		// If beacon is in subset, IGNORE
		// If beacon is not in subset, DELETE

		public void RefreshBeacons()
		{
			var beacons = TerrainBeacons;
		
			int amountToSpawn = preset.amountOfBeacons - beacons.Length;
			if (amountToSpawn <= 0) return; // Do not spawn additional beacons if there are some available

			var directories = Library.ALL_DIRECTORIES.Where(dir => Directory.Exists(dir)).ToArray(); // Valid directories in file sys
			
			var subdirectories = directories;
			if (directories.Length > 3) subdirectories = directories.PickRandomSubset(3).ToArray();

			var _files = subdirectories.Select(subdir => Files.GetFiles(subdir));
			
			var files = new List<FileSystemEntry>();
			foreach(FileSystemEntry[] entries in _files)
				files.AddRange(entries);
			
			amountToSpawn = Mathf.Min(files.Count, amountToSpawn);
			if (amountToSpawn == 0) return; // Ignore request to create more files, aren't enough!

			for (int i = 0; i < amountToSpawn; i++) 
			{
				var _file = files.ElementAt(Random.Range(0, files.Count()));
				CreateBeacon(_file.Path, Type.Desktop, Locale.Terrain); // Create beacon from file
			}

			return;
			
			//DeleteDeprecatedBeacons(lib);

			/*
			var desktop = Library.UserFiles;
			var enviro = Library.WorldFiles;

			var files = desktop.Concat(enviro);
			var blacklist = PlantedBeacons.Select(b => b.File);

			files = files.Except(blacklist); // Ignore items from blacklist

			var composite = files.ToArray();
			var subset = composite.PickRandomSubset<string>(maxBeacons).ToList(); // Random subset from aggregate collection    

			var current = (beacons != null) ? beacons.Keys.ToList() : new List<string>();
			if (current.Count > 0) {

				var target = subset;
				for (int i = 0; i < current.Count; i++) 
				{
					var path = current[i];

					if (!target.Contains(current[i])) {

						var bs = beacons[path].ToArray();
						for (int j = 0; j < bs.Length; j++) 
						{
							var b = bs[j];
							DeleteBeacon(b);
						}

					}
				}

			}
        
			desktop = desktop.Intersect(subset).ToArray();
			enviro = enviro.Intersect(subset).ToArray();
        
			CreateBeacons(desktop, Type.Desktop, Locale.Terrain);
			CreateBeacons(enviro, Type.External, Locale.Terrain);
			*/
		}

		public void RestoreBeacons(BeaconData[] data, bool deprecate = false)
		{
			Debug.LogFormat("Recovered {0} beacons from save!", data.Length);

			if (data == null || data.Length == 0) return; // Ignore restore if data is empty
			
			for (int i = 0; i < data.Length; i++) 
			{
				var beacon = data[i];

				string path = "";
				bool success = Library.Instance.FetchFile(beacon.path, out path);

				if (success) 
				{
					string p = path;
					Vector3 loc = new Vector3(beacon.x, beacon.y, beacon.z) * Constants.BeaconSnapDistance;
					Type t = (Type) beacon.type;
					Locale s = (Locale) beacon.state;

					Debug.LogFormat("Success restore beacon!  file= {0}  locale={1}", p, s);

					var @params = new Hashtable() {
						{"position", loc},
						{"origin", loc}
					};

					var instance = CreateBeacon(p, t, s, @params, fromSave: true);
					if (instance == null)
						continue; // Bypass null beacon

					if (s == Locale.Nest)
						onActivatedBeacon(instance); // Send to nest if immediately found
				}
			}

			if (deprecate) 
			{
				// DeleteDeprecatedBeacons(lib);
			}
		}

		public void WipeBeacons()
		{
			KeyValuePair<string, List<Beacon>>[] cache = beacons.ToArray(); Debug.LogWarning($"Found {cache.Length} beacons to wipe!");
			foreach (KeyValuePair<string, List<Beacon>> lookup in cache) 
			{
				var file = lookup.Key;
				var beacons = lookup.Value.ToArray();

				foreach (Beacon b in beacons) 
				{
					if(b.IsOnFire) DeleteBeacon(b, overridenest:true);    
				}
			}
		}

		public void CreateBeacons(string[] files, Type type, Locale state)
		{
			List<Beacon> instances = new List<Beacon>();
			for (int i = 0; i < files.Length; i++) {
				var path = files[i];

				bool exists = (beacons.ContainsKey(path));
				if (exists) {
					exists = false;

					var bs = beacons[path];
					for (int j = 0; j < bs.Count; j++) {
						if (!Nest.HasBeacon(bs[j])) {
							exists = true;
							break;
						}
					}
				}

				if (!exists) CreateBeacon(files[i], type, state, fromSave: false);
			}
		}

		public void DeleteDeprecatedBeacons(Library lib)
		{
			var current = (beacons != null) ? beacons.Keys.ToList() : new List<string>();

			var path = "";
			for (int i = 0; i < current.Count; i++) {
				path = current[i];

				if (!lib.ContainsFile(path)) {
					var bs = beacons[path].ToArray();
					foreach (Beacon b in bs)
						DeleteBeacon(b, true);
				}
			}
		}

		public Beacon FetchRandomBeacon(bool active = false)
		{
			var beacons = this.beacons.Values;
			IEnumerable<Beacon> collection = (active) ? ActiveBeacons : InactiveBeacons;

			int count = collection.Count();
			if (count == 0) return null;

			var _beacon = collection.ElementAt(Random.Range(0, count));
			return _beacon;
		}

		#endregion

		#region Beacon callbacks

		void onRegisterBeacon(Beacon beacon)
		{
			var file = beacon.File;
			if (beacons.ContainsKey(file)) 
			{
				var list = beacons[file];
				list.Add(beacon);

				beacons[file] = list;
			}
			else
				beacons.Add(file, new List<Beacon>(new Beacon[] { beacon }));

			allBeacons.Add(beacon);
			if (onUpdateBeacons != null)
				onUpdateBeacons();
		}

		void onUnregisterBeacon(Beacon beacon)
		{
			var file = beacon.File;
			if (beacons.ContainsKey(file)) 
			{
				var curr = beacons[file];
				curr.Remove(beacon);

				if (curr.Count == 0) beacons.Remove(file);
				else beacons[file] = curr;
			}

			allBeacons.Remove(beacon);
		
			TriggerUpdateBeacons();
		
			beacon.GetComponent<PoolObject>().Dispose();
			//Destroy(beacon.gameObject);
		}

		void onActivatedBeacon(Beacon beacon)
		{
			Nest.AddBeacon(beacon);
		
			Events.ReceiveEvent(EVENTCODE.BEACONACTIVATE, AGENT.User, AGENT.Beacon, details: beacon.File);
		
			TriggerUpdateBeacons();
		}

		void onDeactivatedBeacon(Beacon beacon)
		{
			TriggerUpdateBeacons();
		}

		void onPlantedBeacon(Beacon beacon)
		{
			Events.ReceiveEvent(EVENTCODE.BEACONPLANT, AGENT.User, AGENT.Beacon, details: beacon.File);
	    
			TriggerUpdateBeacons();
			if (onPlantBeacon != null)
				onPlantBeacon(beacon);
		}

		void onFloweredBeacon(Beacon beacon)
		{
			Events.ReceiveEvent(EVENTCODE.BEACONFLOWER, AGENT.User, AGENT.Beacon, details: beacon.File);
			TriggerUpdateBeacons();
		}

		void onDestroyedBeacon(Beacon beacon)
		{
			var file = beacon.File;
			var others = beacons[file];
			
			onDestroyBeacon.Invoke();

			foreach (Beacon b in others) 
			{
				if (b != beacon && (b.state == Locale.Terrain || b.state == Locale.Nest)) 
				{
					b.Fire();    
				}    
			}
		}

		void onFireBeacon(Beacon beacon, bool self)
		{
			if (!self) 
			{
				var position = beacon.transform.position;
				var instance = firePool.Request();
				instance.transform.position = position;
				instance.GetComponent<ParticleSystem>().Play();
			}
		}

		void onExtinguishBeacon(Beacon beacon, bool self)
		{
			var position = beacon.transform.position;
			var instance = extinguishPool.Request();
				instance.transform.position = position;
				instance.GetComponent<ParticleSystem>().Play();
		}

		void TriggerUpdateBeacons()
		{
			if (onUpdateBeacons != null)
				onUpdateBeacons();
		}

		#endregion
	
		#region Library callbacks

		void UserDeletedFiles(string[] files)
		{
			/*
		foreach (string file in files) 
		{
			if (beacons.ContainsKey(file)) 
			{
				var list = beacons[file];
				foreach(Beacon b in list)
					b.Destroy();	
			}	
		}
		*/
		}

		void UserRecoveredFiles(string[] files)
		{
			/*
		foreach (string file in files) 
		{
			if (beacons.ContainsKey(file)) 
			{
				var list = beacons[file];
				foreach(Beacon b in list)
					b.Recover();
			}	
		}
		*/
		}
	
		#endregion
	
		#region Save/load

		public Object Save()
		{
			BeaconSceneData _dat = new BeaconSceneData();
				
			var dat = new List<BeaconData>();
			for (var i = 0; i < allBeacons.Count; i++) 
			{
				var beacon = allBeacons[i];
				var index = Library.Instance.FetchFileIndex(beacon.File);
				var parsed = new BeaconData((ushort)index, beacon.Origin, beacon.type, beacon.state);

				dat.Add(parsed);
			}
			_dat.beacons = dat.ToArray();
		
			return _dat;
		}

		public void Load(Object dat)
		{
			maxBeacons = preset.amountOfBeacons; // Initialize max beacons

			World = World.Instance;
			Nest = Nest.Instance;
			SaveSys = GameDataSaveSystem.Instance;
			Library = Library.Instance;
			Files = FileNavigator.Instance;

			if (dat != null) 
			{
				var beacons = (BeaconSceneData) dat;
				RestoreBeacons(beacons.beacons);
			}
		}

		#endregion

		#region Transitions

		public void OnBeginFlower(Beacon beacon, Vector3 position)
		{
			//Instantiate(impactPS, position, Quaternion.identity); // Spawn impact!
			beacon.Trails.enabled = true;
			//beacon.Trails.autodestruct = true;
			
			beacon.OnFlowerSpawn.Invoke();
		}

		public void OnEndFlower(Beacon beacon, Vector3 position)
		{
			//Instantiate(impactPS, position, Quaternion.identity); // Spawn impact!
			//beacon.Trails.autodestruct = false;
			beacon.Trails.enabled = false;
		}

		#endregion
	
		#region Debug

		void DebugBeaconFromDesktop()
		{
			if (!preset.useDesktopFilesForDebugBeacons) 
			{
				var _files = preset.defaultTextures;
				var _file = _files[Random.Range(0, _files.Length)];

				CreateBeacon(_file.name + ".jpg", Type.World, Locale.Terrain, transition: TransitionType.Spawn,
					fromSave: false);
			}
			else {
				var files = Files.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));	
			
				var index = Random.Range(0, files.Length);
				var file = files[index].Path;

				CreateBeacon(file, Beacon.Type.Desktop, Beacon.Locale.Terrain, fromSave: false, transition: TransitionType.Spawn);
			}
		}
	
		#endregion
	}
}
