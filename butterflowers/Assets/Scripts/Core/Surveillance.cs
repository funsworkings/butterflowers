using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Settings;
using UnityEngine;
using uwu;
using uwu.Extensions;

public class Surveillance : MonoBehaviour
{
	public static Surveillance Instance = null;
	
	// External

	GameDataSaveSystem Save;
	Library Lib;

	[SerializeField] Sun Sun;
	[SerializeField] ButterflowerManager ButterflyManager;
	[SerializeField] Nest Nest;
	[SerializeField] Wand Wand;
	[SerializeField] Focusing Focus;
	
	// Collections

	[SerializeField] List<SurveillanceData> logs = new List<SurveillanceData>();
	[SerializeField] List<SurveillanceLogData> cacheLogs = new List<SurveillanceLogData>();
	
	// Properties
	
	[SerializeField] WorldPreset Preset;   
	
	
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
	
	#region Initialization

	public void Load(SurveillanceData[] data)
	{
		Save = GameDataSaveSystem.Instance;
		Lib = Library.Instance;
		
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

			if (Sun.active) 
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
			log.cursorSpeed = Wand.speed;
			log.nestFill = Nest.fill;

			var focus = Focus.focus;
			if (focus != null)
				log.agentInFocus = focus.Agent;
			else
				log.agentInFocus = AGENT.NULL; // No agent currently in focus

		return log;
	}
	
	#endregion
	
	#region Recording events

	void OnReceiveEvent(EVENTCODE @event, AGENT agentA, AGENT agentB, string details)
	{
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
