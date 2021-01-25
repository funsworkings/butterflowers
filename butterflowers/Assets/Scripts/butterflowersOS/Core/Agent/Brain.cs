using System;
using butterflowersOS.Core.Agent.Modules;
using butterflowersOS.Data;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Managers;
using butterflowersOS.Presets;
using UnityEngine;
using uwu;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Core.Agent
{
	public class Brain : MonoBehaviour
	{
		// External

		GameDataSaveSystem _Save;
		
		[Header("External")]
		
		[SerializeField] Sun Sun;
		[SerializeField] World World;
		[SerializeField] Nest Nest;

		[SerializeField] Surveillance Surveillance;
		[SerializeField] BeaconManager Beacons;
		[SerializeField] VineManager Vines;
		
		// Properties

		FOV fov;

		[Header("Properties")] 
			[SerializeField] WorldPreset preset;

		[Header("Elements")]
			[SerializeField] ToggleOpacity uiPanel;

		[Header("Data")] 
			[SerializeField] SurveillanceData data = null;
			[SerializeField] int dataStackHeight = -1;
			
		[Header("Behaviours")]
			[SerializeField] float refresh_t = 0f;
			[SerializeField] int logIndex = 0;


		void Awake()
		{
			fov = GetComponent<FOV>();
		}
		
		
		void OnEnable()
		{
			if (_Save == null) _Save = GameDataSaveSystem.Instance;
			
			Initialize();

			Surveillance.onCaptureLog += ResetCurrentLog;
			
			uiPanel.Show();
		}

		void OnDisable()
		{
			Surveillance.onCaptureLog -= ResetCurrentLog;
			
			uiPanel.Hide();
		}

		void Update()
		{
			if (Surveillance.recording) // Trigger behaviours when surveillance is active
			{
				refresh_t += Time.deltaTime;

				if (refresh_t > preset.surveillanceLogRate) 
				{

				}
			}
		}
		
		#region Ops

		void Initialize()
		{
			FetchStackHeight(); // Get event stack height
			FetchSurveillanceData(); // Get current surveillance data

			refresh_t = 0f; // Reset refresh time
			logIndex = 0;
		}

		#endregion

		#region Event stack

		void FetchStackHeight()
		{
			dataStackHeight = _Save.data.agent_event_stack;
		}

		void FetchSurveillanceData()
		{
			int sceneDataLogIndex = Surveillance.activeLogIndex;
			int dataLogIndex = (int)Mathf.Repeat(sceneDataLogIndex, dataStackHeight);
			
			data = Surveillance.FetchLog(dataLogIndex);
		}
		
		#endregion
		
		#region Event logs

		void ResetCurrentLog()
		{
			
		}
		
		#endregion
	}
}