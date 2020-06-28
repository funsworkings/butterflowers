using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using System.Linq;

public class Scribe : Logger
{
	public static Scribe Instance = null;

	#region External

	GameDataSaveSystem Save;

	#endregion

	#region Internal

	[System.Serializable]
	public class Log 
	{
		public byte paramx, paramy, paramz;
		public string detail;
	}

	#endregion

	#region Properties

	RectTransform rect;

	[SerializeField] FocalPoint focus;

	[SerializeField] UIExt.Behaviors.Visibility.ToggleOpacity controls;
	[SerializeField] GameObject upArrow, downArrow;

	[SerializeField] Text scroll;
	[SerializeField] UIExt.Elements.ScrollRectNavigator navigator;

	#endregion

	#region Collections

	public List<Log> save_caches = new List<Log>();
	public List<Log> caches = new List<Log>();

	#endregion

	#region Attributes

	string log = "";
	[SerializeField] Color defaultColor;
	string defaultColorHex;

	[SerializeField] int maxLines = 16;
	[SerializeField] int position = 0;

	#endregion

	#region Monobehaviour callbacks

	void Awake()
	{
		duplicates = true;
		auto = true;
		resize = true;

		Instance = this;
		Save = GameDataSaveSystem.Instance;

		rect = GetComponent<RectTransform>();

		defaultColorHex = Extensions.ParseColor(defaultColor);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.S)) {
			string msg = Extensions.RandomString(Random.Range(16, 64));
			Push(msg);
		}
	}

	void OnEnable() 
	{
		Events.onFireEvent += Push;

		focus.onFocus += onFocus;
		focus.onLoseFocus += onLoseFocus;
	}

	void OnDisable()
	{
		Events.onFireEvent -= Push;

		focus.onFocus -= onFocus;
		focus.onLoseFocus -= onLoseFocus;

		Clear();
	}

	#endregion

	#region Logger overrides

	protected override void onPushElement(string message)
	{
		if (string.IsNullOrEmpty(log))
			log += message;
		else
			log += string.Format("\n{0}", message);

		if (!focus.isFocused) {
			position = caches.Count - 1;
		}
		else 
		{
			if (position == caches.Count - 2)
				position = caches.Count - 1;
		}

		UpdateCrop();
	}

	#endregion

	#region Focus callbacks

	void onFocus()
	{
		controls.Show();
	}

	void onLoseFocus()
	{
		controls.Hide();
	}

	#endregion

	#region Logs

	public void Restore(Log[] logs)
	{
		if (logs == null) return;

		save_caches = new List<Log>(logs);
		for (int i = 0; i < logs.Length; i++)
			Push(logs[i], save:false);
	}

	public void Push(EVENTCODE @event, AGENT a, AGENT b, string detail)
	{
		Push(@event, a, b, detail, true);
	}

	public void Push(EVENTCODE @event, AGENT a, AGENT b, string detail, bool save = true)
	{
		Log log = new Log();
		log.paramx = (byte)@event;
		log.paramy = (byte)a;
		log.paramz = (byte)b;
		log.detail = detail;

		Push(log, save);
	}

	void Push(Log log, bool save = true)
	{
		if (save) save_caches.Add(log);
		caches.Add(log);

		Save.logs = save_caches.ToArray();
		Push(parseLog(log));
	}

	#endregion

	#region Size and position

	void UpdateCrop()
	{
		string[] entries = log.Split('\n');
		int count = entries.Length;

		int lines = Mathf.Min(count, maxLines);

		position = Mathf.Clamp(position, lines, count - 1);
		UpdateControls(lines);

		int crop = Mathf.Max(0, position - lines);
		string final = string.Join("\n", entries.SubArray(crop, lines));

		scroll.text = final;
	}

	void UpdateControls(int lines)
	{
		upArrow.SetActive(position != lines);
		downArrow.SetActive(position != caches.Count - 1);
	}

	void ClampPosition()
	{
		position = Mathf.Clamp(position, 0, caches.Count - 1);
	}

	public void MoveUp(int amount)
	{
		position -= amount;
		ClampPosition();

		UpdateCrop();
	}

	public void MoveDown(int amount)
	{
		position += amount;
		ClampPosition();

		UpdateCrop();
	}

	#endregion

	#region Helpers

	private string parseLog(Log log)
	{
		var detail = log.detail;

		var @event = (EVENTCODE)((int)log.paramx);
		var agA = (AGENT)((int)log.paramy);
		var agB = (AGENT)((int)log.paramz);

		string e = parseEventcode(@event);
		string agent_a = formatAgent(agA);
		string agent_b = formatAgent(agB, detail);

		if (@event == EVENTCODE.UNKNOWN) // ??????????
		{
			/*string color = "";

			bool success = COLOR_LOOKUP.AGENTS.TryGetValue(agA, out color);
			if (!success) color = defaultColorHex;

			string a = string.Format("<color={0}>{1}<color={2}>", color, detail, defaultColorHex);*/
			return detail;
		}

		// NORMAL PARSE
		return string.Format("{2} was {1} by {0}<i>!</i>", agent_a, e, agent_b);
	}

	private string formatAgent(AGENT agent, string detail = null)
	{
		if (agent == AGENT.NULL) return null;

		string @base = System.Enum.GetName(typeof(AGENT), agent).ToUpper();
		if (agent == AGENT.Beacon && !string.IsNullOrEmpty(detail))
			@base = detail;

		string color = "";

		bool success = COLOR_LOOKUP.AGENTS.TryGetValue(agent, out color);
		if (!success) color = defaultColorHex;

		string a = string.Format("<color={0}>{1}</color>", color, @base);
		return a;
	}

	private string parseEventcode(EVENTCODE @event)
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
			default:
				break;
		}

		return e;
	}

	#endregion
}
