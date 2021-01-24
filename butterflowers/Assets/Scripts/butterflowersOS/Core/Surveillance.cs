using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Data;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Managers;
using butterflowersOS.Presets;
using Neue.Agent.Brain.Data;
using Neue.Reference.Types;
using Neue.Reference.Types.Maps;
using Neue.Reference.Types.Maps.Groups;
using UnityEngine;
using uwu.Camera;
using uwu.Extensions;
using uwu.Snippets;

namespace butterflowersOS.Core
{
	public class Surveillance : MonoBehaviour
	{
		public static Surveillance Instance = null;
	
		// Events

		public System.Action onCaptureLog;
	
		// External
	
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
		[SerializeField] List<EVENTCODE> eventsDuringLog = new List<EVENTCODE>();

		// Properties
	
		[SerializeField] WorldPreset Preset;
		[SerializeField] global::Neue.Agent.Presets.BrainPreset BrainPreset;
	
		public bool recording = true;

		Camera previousMainCamera;

		public string lastPhotoCaption, lastPhotoPath;
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
		
		#region Ops

		public void New(bool onload = false)
		{
			if (onload) 
			{
				EnsureLog();
			}
			else 
			{
				CreateLog();
			}

			StartCoroutine("Capturing");
			recording = true;
		}

		public void Stop()
		{
			StopAllCoroutines();
		}

		public void Dispose()
		{
			StartCoroutine("Disposal");
		}

		IEnumerator Disposal()
		{
			TakePicture();
			
			photoInProgress = true;
			while (photoInProgress) 
				yield return null;

			yield return new WaitForEndOfFrame();

			if (recording) 
			{
				var log = CaptureFrameLog();
				activeLog.logs = activeLog.logs.Append(log).ToArray();

				recording = false;
			}
		}
		
		#endregion

		#region Save/load

		public object Save()
		{
			return logs.ToArray();
		}

		public void Load(object data)
		{
			Lib = Library.Instance;
			World = World.Instance;

			var previousLogs = (SurveillanceData[]) data;
			if (data != null)
				logs = new List<SurveillanceData>(previousLogs);
			else
				logs = new List<SurveillanceData>();
			
			SubscribeToEvents();
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
		
		#region Log creation
		
		public void CreateLog()
		{
			var log = new SurveillanceData();
				log.timestamp = (ushort)Sun.days;
				log.logs = new SurveillanceLogData[]{}; // Wipe daily logs

			logs.Add(log); // Append new log to 
		}

		void EnsureLog()
		{
			bool flagCreate = false;
		
			var timestamp = Sun.days;
		
			SurveillanceData lastLog = (logs != null && logs.Count > 0)? logs.Last():null;
			if (lastLog != null) 
			{
				var lastTimestamp = lastLog.timestamp;
				if (lastTimestamp != timestamp)
					flagCreate = true;
			}
			else
				flagCreate = true;
		
			if(flagCreate) CreateLog();
		}
		
		#endregion
	
		#region Capturing

		IEnumerator Capturing()
		{
			float log_t = 0f;
		
			while (true) 
			{
				log_t += Time.deltaTime;
				
				if (log_t > Preset.surveillanceLogRate) 
				{
					var log = CaptureFrameLog();
					activeLog.logs = activeLog.logs.Append(log).ToArray();

					if (onCaptureLog != null)
						onCaptureLog();

					log_t = 0f;
				}

				yield return null;
			}
		}

		SurveillanceLogData CaptureFrameLog()
		{
			var log = new SurveillanceLogData();

			log.butterflyHealth = (byte)( ButterflyManager.GetHealth() * 255f);

			var cursorX = Wand.velocity2d.x;
			var cursorY = Wand.velocity2d.y;

			log.cursorX = (sbyte) (cursorX / Constants.BaseCursorVelocityVector);
			log.cursorY = (sbyte) (cursorY / Constants.BaseCursorVelocityVector);
				
			log.nestFill = (byte)(Nest.fill * 255f);

			var focus = Focus.focus;
			if (focus != null)
				log.agentInFocus = focus.Agent.ToByte();
			else
				log.agentInFocus = AGENT.NULL.ToByte(); // No agent currently in focus
			
			AttachEventsToLog(ref log);
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

			if (Preset.takePhotos) 
			{
				lastPhotoPath = Lib.CreateFile(name,
					System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop), image,
					Library.FileType.Shared);
			}
			else
				lastPhotoPath = null;

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
				comp.filesAdded = (ushort) logs.Select(l => (int)l.filesAdded).Average();
				comp.filesRemoved = (ushort) logs.Select(l => (int)l.filesRemoved).Average();
				comp.Discoveries = (int) logs.Select(l => l.discoveries).Average();

				comp.BeaconsAdded = (int) logs.Select(l => l.beaconsAdded).Average();
				comp.BeaconsDestroyed = (int) logs.Select(l => l.beaconsDestroyed).Average();
				comp.BeaconsPlanted = (int) logs.Select(l => l.beaconsPlanted).Average();
				comp.BeaconsFlowered = (int) logs.Select(l => l.beaconsFlowered).Average();

				comp.NestKicks = (int) logs.Select(l => l.nestKicks).Average();
				comp.NestSpills = (int) logs.Select(l => l.nestSpills).Average();
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
	
		#region Profiling
	
		public Profile ConstructBehaviourProfile()
		{
			var profile = new Profile();

			var compositeLog = CreateCompositeAverageLog(true);
			var logs = allLogs;

			var maps = new List<FrameFloat>();

			var behaviours = System.Enum.GetValues(typeof(Frame));
			foreach (int behaviourVal in behaviours) {
				var b = (Frame) behaviourVal;

				var map = new FrameFloat();
				map.frame = b;
				map.value = CalculateBehaviourWeightForProfile(b, compositeLog, logs);

				maps.Add(map);
			}

			var group = new FrameFloatGroup();
			group.behaviours = maps;

			profile.weights = group;
			return profile;
		}
	
		float CalculateBehaviourWeightForProfile(Frame frame, CompositeSurveillanceData composite,
			SurveillanceData[] history)
		{
			var delta = new SurveillanceDataDelta(Preset.baselineSurveillanceData, composite);
			var factors = new List<float>();
		
			switch (frame) {
				case Frame.Order:
					factors.AddRange(new float[] {
						1f - delta.discoveries,
						delta.hob,
						1f - delta.cursorspeed,
						1f - delta.volatility,
						1f - delta.nestKicks
					});

					break;
				case Frame.Quiet:
					factors.AddRange(new float[] {
						1f - delta.volatility,
						delta.beaconsPlanted,
						1f - delta.nestKicks
					});

					break;
				case Frame.Nurture:
					factors.AddRange(new float[] {
						delta.beaconsPlanted,
						1f - delta.beaconsAdded,
						delta.hob,
						delta.nestfill,
						delta.filesAdded
					});

					break;
				case Frame.Destruction:
					factors.AddRange(new float[] {
						delta.filesRemoved,
						1f - delta.hob,
						1f - delta.nestfill,
						delta.cursorspeed,
						delta.beaconsAdded,
						delta.nestSpills
					});

					break;
			}

			var average = factors.Average() / BrainPreset.baselineDeltaPercentage;
			var avg = average.RemapNRB(-1f, 1f, 0f, 1f);

			return avg;
		}
	
		#endregion
		
		#region Neueagent

		public bool AggregateNeueAgentData(SurveillanceData[] data)
		{
			SurveillanceData[] _cacheLogs = logs.ToArray();
			
			ushort diff = (ushort)(data.Length - logs.Count);
			if (diff > 0) 
			{
				for (int i = 0; i < diff; i++) 
				{
					logs.Add(new SurveillanceData());	// Pad current logs with extended
				}	
			}

			bool success = true;
			
			for (int i = 0; i < data.Length; i++) 
			{
				var log = logs[i]; // Grab overlapped log
				var newLog = data[i];
				
				success = log.AggregateSurveillanceData(newLog);
				if (!success) break;
			}

			if (!success) 
			{
				logs = new List<SurveillanceData>(_cacheLogs); // Reset back to stable logs
			}

			return success;
		}
		
		#endregion
	
		#region Events

		void OnReceiveEvent(EVENTCODE @event, AGENT agentA, AGENT agentB, string details)
		{
			if (!recording) return;
			eventsDuringLog.Add(@event);
		}

		void AttachEventsToLog(ref SurveillanceLogData log)
		{
			EVENTCODE[] events = eventsDuringLog.ToArray();
			log.AppendEvents(events);
		
			eventsDuringLog = new List<EVENTCODE>(); // Wipe event cache
		}

		#endregion
	
		#region Library callbacks

		void onAddedFiles(string[] files)
		{
			activeLog.filesAdded = (ushort)files.Length;
		}

		void onRemovedFiles(string[] files)
		{
			activeLog.filesRemoved = (ushort)files.Length;
		}
	
		#endregion
	}
}
