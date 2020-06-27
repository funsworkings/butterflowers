using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;
using System.Linq;

public class Discovery: Singleton<Discovery> {

	#region Events

	public static System.Action<string> onDiscover;

	#endregion

	#region External

	GameDataSaveSystem Save;
	public Settings.WorldPreset Preset = null;

	#endregion

	#region Collections

	[SerializeField] List<string> discoveries = new List<string>();

	#endregion

	#region Attributes

	bool m_load = false;
	public bool load {
		get
		{
			return m_load;
		}
	}

	#endregion

	void Awake()
	{
		Save = GameDataSaveSystem.Instance;
	}

	IEnumerator Start() {
		while (!Save.load)
			yield return null;

		var d = Save.discovered;
		discoveries = new List<string>(d);

		if (!Preset.persistDiscoveries) discoveries = new List<string>(); // Reset memories

		m_load = true;
	}

	public bool HasDiscoveredFile(string path)
	{
		return discoveries.Contains(path);
	}

	public bool DiscoverFile(string path)
	{
		if (HasDiscoveredFile(path)) return false;

		discoveries.Add(path);
		SendDiscoveries();

		if (onDiscover != null)
			onDiscover(path);

		return true;
	}

	void SendDiscoveries()
	{
		Save.discovered = discoveries.ToArray();
	}
}