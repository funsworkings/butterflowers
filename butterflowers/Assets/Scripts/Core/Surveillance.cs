using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Settings;
using UnityEngine;
using uwu;
using uwu.Camera;
using uwu.Extensions;
using uwu.Snippets;

public class Surveillance : MonoBehaviour, IReactToSunCycle
{
	public static Surveillance Instance = null;
	
	// External

	GameDataSaveSystem Save;
	Library Lib;
	World World;

	[SerializeField] Sun Sun;
	[SerializeField] ButterflowerManager ButterflyManager;
	[SerializeField] CameraManager CameraManager;
	[SerializeField] Snapshot snapshotCamera;
	[SerializeField] Nest Nest;
	[SerializeField] Wand Wand;
	[SerializeField] Focusing Focus;

	// Collections

	[SerializeField] List<SurveillanceData> logs = new List<SurveillanceData>();

	// Properties
	
	[SerializeField] WorldPreset Preset;
	[SerializeField] bool recording = true;

	Camera previousMainCamera;
	
	public string lastPhotoCaption;
	public Texture2D lastPhotoTaken;
	public bool photoInProgress = false;
	
	#region Accessors

	public SurveillanceData activeLog => (logs.Count > 0) ? logs.Last() : null;

	public SurveillanceData[] previousLogs
	{
		get
		{
			if (logs.Count > 1) 
			{
				var cache = new SurveillanceData[logs.Count - 1];
				Array.Copy(logs.ToArray(), cache, cache.Length);

				return cache;
			}

			return null;
		}
	}

	public SurveillanceData[] allLogs => logs.ToArray();

	#endregion
	
	#region Monobehaviour callbacks

	void Awake()
	{
		Instance = this;
	}

	void OnDestroy()
	{
		StopAllCoroutines();
		UnsubscribeFromEvents();
	}

	#endregion

	#region Cycle

	public void Cycle(bool refresh)
	{
		TakePicture();
		CreateLog();
	}

	#endregion
	
	#region Initialization

	public void Load(SurveillanceData[] data)
	{
		Save = GameDataSaveSystem.Instance;
		Lib = Library.Instance;
		World = World.Instance;
		
		if (data != null) 
		{
			logs = new List<SurveillanceData>(data);
		}
		else 
		{
			logs = new List<SurveillanceData>();
		}
		EnsureLog();
		
		SubscribeToEvents();
		
		StartCoroutine("Capturing");
	}
	
	#endregion
	
	#region Event subscriptions

	void SubscribeToEvents()
	{
		Lib.onAddedFiles += onAddedFiles;
		Lib.onRemovedFiles += onRemovedFiles;

		Events.onFireEvent += OnReceiveEvent;
	}

	void UnsubscribeFromEvents()
	{
		Lib.onAddedFiles -= onAddedFiles;
		Lib.onRemovedFiles -= onRemovedFiles;

		Events.onFireEvent -= OnReceiveEvent;
	}
	
	#endregion
	
	#region Capturing

	IEnumerator Capturing()
	{
		float log_t = 0f;
		
		while (true) {
			recording = (World.state != World.State.Remote); // Only record user/parallel actions
			if (Sun.active && recording) 
			{
				log_t += Time.deltaTime;
				if (log_t > Preset.surveillanceLogRate) {
					var log = CaptureFrameLog();
					activeLog.logs = activeLog.logs.Append(log).ToArray();

					Save.data.surveillanceData = logs.ToArray(); // Update all surveillance data in save

					log_t = 0f;
				}
			}

			yield return null;
		}
	}

	public void CreateLog()
	{
		var log = new SurveillanceData();
			log.timestamp = Sun.days;
			
			log.logs = new SurveillanceLogData[]{}; // Wipe daily logs

		logs.Add(log); // Append new log to 
	}

	void EnsureLog()
	{
		bool flagCreate = false;
		
		var timestamp = Sun.days;
		
		SurveillanceData lastLog = (logs != null && logs.Count > 0)? logs.Last():null;
		if (lastLog != null) {
			var lastTimestamp = lastLog.timestamp;
			if (lastTimestamp != timestamp)
				flagCreate = true;
		}
		else
			flagCreate = true;
		
		if(flagCreate) CreateLog();
	}

	SurveillanceLogData CaptureFrameLog()
	{
		var log = new SurveillanceLogData();
			log.timestamp = Sun.time;

			log.butterflyHealth = ButterflyManager.GetHealth();
			log.cursorSpeed = Wand.speed2d;
			log.nestFill = Nest.fill;

			var focus = Focus.focus;
			if (focus != null)
				log.agentInFocus = focus.Agent;
			else
				log.agentInFocus = AGENT.NULL; // No agent currently in focus

		return log;
	}
	
	#endregion
	
	#region Pictures
	
	void TakePicture()
	{
		var camera = snapshotCamera.camera;

		previousMainCamera = CameraManager.MainCamera;
		CameraManager.MainCamera = camera;
		camera.enabled = true;

		snapshotCamera.onSuccess += onReceivePicture;

		photoInProgress = true;
		StartCoroutine("TakingPicture");
	}

	IEnumerator TakingPicture()
	{
		yield return new WaitForSecondsRealtime(.167f);
		snapshotCamera.Capture();
	}

	void onReceivePicture(Texture2D image)
	{
		var name = Extensions.RandomString(12);
		var camera = snapshotCamera.camera;

		if(Preset.takePhotos)
			Lib.SaveTexture(name, image);

		lastPhotoCaption = name;
		lastPhotoTaken = image;

		CameraManager.MainCamera = previousMainCamera;

		camera.enabled = false;
		previousMainCamera = null;

		snapshotCamera.onSuccess -= onReceivePicture;

		Events.ReceiveEvent(EVENTCODE.PHOTOGRAPH, AGENT.World, AGENT.Terrain);

		photoInProgress = false;
	}
	
	
	#endregion
	
	#region Compositing
	
	public CompositeSurveillanceData CreateCompositeAverageLog(bool includeSelf = false)
	{
		CompositeSurveillanceData comp = new CompositeSurveillanceData();
		
		var logs = (includeSelf)? allLogs: previousLogs;
		if (logs == null) 
		{
			comp = new CompositeSurveillanceData(activeLog);
			logs = new SurveillanceData[] { activeLog };
		}
		else 
		{
			comp.filesAdded = (int) logs.Select(l => l.filesAdded).Average();
			comp.filesRemoved = (int) logs.Select(l => l.filesRemoved).Average();
			comp.discoveries = (int) logs.Select(l => l.discoveries).Average();

			comp.beaconsAdded = (int) logs.Select(l => l.beaconsAdded).Average();
			comp.beaconsPlanted = (int) logs.Select(l => l.beaconsPlanted).Average();

			comp.nestKicks = (int) logs.Select(l => l.nestKicks).Average();
			comp.nestSpills = (int) logs.Select(l => l.nestSpills).Average();
		}

		comp.AverageCursorSpeed = logs.Select(l => l.averageCursorSpeed).Average();
		comp.AverageHoB = logs.Select(l => l.averageHoB).Average();
		comp.AverageNestFill = logs.Select(l => l.averageNestFill).Average();

		comp.AverageTimeSpentInNest = logs.Select(l => l.timeSpentInNest).Average();
		comp.AverageTimeSpentInTree = logs.Select(l => l.timeSpentInTree).Average();
		comp.AverageTimeSpentInMagicStar = logs.Select(l => l.timeSpentInMagicStar).Average();
		comp.AverageTimeSpentInDefault = logs.Select(l => l.timeSpentInDefault).Average();
				

		return comp;
	}
	
	#endregion
	
	#region Recording events

	void OnReceiveEvent(EVENTCODE @event, AGENT agentA, AGENT agentB, string details)
	{
		if (!recording) return;
		if (agentA == AGENT.User) 
		{
			RecordEvent(@event); // Only record events from User
		}
	}

	public void RecordEvent(EVENTCODE @event)
	{
		switch (@event) {
			case EVENTCODE.NESTKICK:
				activeLog.nestKicks++;
				break;
			case EVENTCODE.NESTSPILL:
				activeLog.nestSpills++;
				break;
			case EVENTCODE.DISCOVERY:
				activeLog.discoveries++;
				break;
			case EVENTCODE.BEACONPLANT:
				activeLog.beaconsPlanted++;
				break;
			case EVENTCODE.BEACONACTIVATE:
				activeLog.beaconsAdded++;
				break;
			default:
				break;
		}
	}
	
	#endregion
	
	#region Library callbacks

	void onAddedFiles(string[] files)
	{
		activeLog.filesAdded = files.Length;
	}

	void onRemovedFiles(string[] files)
	{
		activeLog.filesRemoved = files.Length;
	}
	
	#endregion
}
