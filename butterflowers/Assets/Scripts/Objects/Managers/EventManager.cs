using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using System.Linq;
using Core;
using Settings;
using UI;
using uwu;
using uwu.Extensions;
using uwu.UI.Behaviors.Visibility;
using uwu.UI.Elements;
using Logger = uwu.Snippets.Logger;

public class EventManager : Logger
{
	// External

	GameDataSaveSystem Save;
	Library Library;

	// Properties

	[SerializeField] WorldPreset World;
	
	[SerializeField] Transform logsContainer;
	[SerializeField] GameObject logPrefab;

	// Collections

	public List<Log> save_caches = new List<Log>();
	public List<Log> caches = new List<Log>();

	public List<EventLog> logBlocks = new List<EventLog>();

	// Attributes
	
	[SerializeField] Color defaultColor;
	string defaultColorHex;

	[SerializeField] EVENTCODE[] excludes = new EVENTCODE[] { };
	
	[SerializeField] float logScrollSpeed = 1f;

	#region Monobehaviour callbacks

	void Awake()
	{
		duplicates = true;
		auto = true;
		resize = true;

		defaultColorHex = Extensions.ParseColor(defaultColor);
	}

	void OnEnable() 
	{
		Events.onFireEvent += OnReceiveEvent;
	}

	void OnDisable()
	{
		Events.onFireEvent -= OnReceiveEvent;

		Clear();
	}

	void Update()
	{
		ScrollLogContainer();
	}

	#endregion
	
	#region Initialization

	public void Load(LogData dat)
	{
		Save = GameDataSaveSystem.Instance;
		Library = Library.Instance;
		
		if (dat == null) return;

		var logs = dat.logs;

		save_caches = new List<Log>(dat.logs);

		for (int i = 0; i < logs.Length; i++) {
			var log = logs[i];
			var detail = log.detail_lookup;

			string info = (detail < 0) ? "" : Library.ALL_FILES[detail];
			Push(logs[i], info, save: false);
		}
	}
	
	#endregion

	#region Logger overrides

	protected override void onPushElement(int index, string message)
	{
		AddLog(message);
	}

	protected override void onPopElement(int index, string message)
	{
		var logInstance = logBlocks[index];
		RemoveLog(logInstance);
	}

	#endregion
	
	#region Event callbacks
	
	public void OnReceiveEvent(EVENTCODE @event, AGENT a, AGENT b, string detail)
	{
		if (!World.logEvents) return; // Ignore log events 

		if (excludes.Contains(@event)) return; //Ignore
		Push(@event, a, b, detail, false);
	}
	
	#endregion

	#region Logging

	public void Push(EVENTCODE @event, AGENT a, AGENT b, string detail, bool save = true)
	{
		Log log = new Log();
			log.paramx = (byte)@event;
			log.paramy = (byte)a;
			log.paramz = (byte)b;

		Push(log, detail, save);
	}

	void Push(Log log, string detail, bool save = true)
	{
		if (!string.IsNullOrEmpty(detail)) 
		{
			int index = Library.ALL_FILES.IndexOf(detail);
			log.detail_lookup = index;
		}
		
		if (save) 
		{
			if (!string.IsNullOrEmpty(detail)) 
			{
				int index = Library.ALL_FILES.IndexOf(detail);
				log.detail_lookup = index;
			}

			save_caches.Add(log);
		}
		caches.Add(log);
		
		Push(parseLog(log));
	}

	#endregion
	
	#region UI logs

	public void AddLog(string message)
	{
		GameObject instance = Instantiate(logPrefab, logsContainer);
		EventLog log = instance.GetComponent<EventLog>();
			log.Initialize(message, 2f, this);
			
		logBlocks.Add(log);
	}

	public void RemoveLog(EventLog log)
	{
		print("Remove " + log.text);
		
		logBlocks.Remove(log);
		GameObject.Destroy(log);
	}

	void ScrollLogContainer()
	{
		float elementHeight = logPrefab.GetComponent<RectTransform>().sizeDelta.y;
		float targetHeight = logBlocks.Count() * elementHeight;

		RectTransform rect = (logsContainer as RectTransform);
		
		Vector2 pos = rect.anchoredPosition;
		rect.anchoredPosition = Vector2.Lerp(pos, new Vector2(pos.x, targetHeight), logScrollSpeed * Time.deltaTime);
	}
	
	#endregion

	#region Helpers

	string parseLog(Log log, string overridedetail = null)
	{
		var @event = (EVENTCODE)((int)log.paramx);
		var agA = (AGENT)((int)log.paramy);
		var agB = (AGENT)((int)log.paramz);

		var detail_lookup = log.detail_lookup;
		var detail = (detail_lookup == -1) ? "" : Library.ALL_FILES[detail_lookup];

		if (!string.IsNullOrEmpty(overridedetail))
			detail = overridedetail;

		string e = parseEventcode(@event);
		string agent_a = formatAgent(agA);
		string agent_b = formatAgent(agB, detail);

		// NORMAL PARSE
		return string.Format("{2} was {1} by {0}<i>!</i>", agent_a, e, agent_b);
	}

	string formatAgent(AGENT agent, string detail = null)
	{
		if (agent == AGENT.NULL) return null;

		string @base = System.Enum.GetName(typeof(AGENT), agent).ToUpper();
		if (agent == AGENT.Beacon) {
			if (string.IsNullOrEmpty(detail))
				detail = "?????????????";
			else
				detail = detail.AbbreviateFilename();

			@base = detail;
		}

		string color = "";

		bool success = COLOR_LOOKUP.AGENTS.TryGetValue(agent, out color);
		if (!success) color = defaultColorHex;

		string a = string.Format("<color={0}>{1}</color>", color, @base);
		return a;
	}

	string parseEventcode(EVENTCODE @event)
	{
		string e = null;
		string col = "";

		switch (@event) {
			case EVENTCODE.UNKNOWN:
				e = "????????????????????????";
				break;
			case EVENTCODE.DISCOVERY:
				e = "<i>DISCOVERED</i>";
				break;

			case EVENTCODE.NESTKICK:
				e = "<i>KICKED</i>";
				break;
			case EVENTCODE.NESTPOP:
				e = "<i>POPPED</i> from NEST";
				break;
			case EVENTCODE.NESTSPILL:
				e = "<i>SPILLED</i>";
				break;
			case EVENTCODE.NESTCLEAR:
				e = "<i>CLEARED</i>";
				break;
			case EVENTCODE.NESTSHRINK:
				e = "<i>SHRUNK</i>";
				break;
			case EVENTCODE.NESTGROW:
				e = "<i>EMBIGGENED</i>";
				break;

			case EVENTCODE.BEACONACTIVATE:
				if (!COLOR_LOOKUP.AGENTS.TryGetValue(AGENT.Nest, out col))
					col = defaultColorHex;

				e = string.Format("<i>ADDED</i> to <color={0}>NEST</color>", col);
				break;
			case EVENTCODE.BEACONADD:
				if (!COLOR_LOOKUP.AGENTS.TryGetValue(AGENT.Terrain, out col))
					col = defaultColorHex;

				e = string.Format("<i>ADDED</i> to <color={0}>TERRAIN</color>", col);
				break;
			case EVENTCODE.BEACONDELETE:
				if (!COLOR_LOOKUP.AGENTS.TryGetValue(AGENT.Terrain, out col))
					col = defaultColorHex;

				e = string.Format("<i>DELETED</i> from <color={0}>TERRAIN</color>", col);
				break;
			case EVENTCODE.BEACONPLANT:
				if (!COLOR_LOOKUP.AGENTS.TryGetValue(AGENT.Terrain, out col))
					col = defaultColorHex;

				e = string.Format("<i>PLANTED</i> IN <color={0}>TERRAIN</color>", col);
				break;
			case EVENTCODE.PHOTOGRAPH:
				e = "<i>CAPTURED</i>";
				break;
			default:
				break;
		}

		return e;
	}

	#endregion
}
