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
	Library Library;

	public Settings.WorldPreset Preset = null;

	#endregion

	#region Collections

	public List<int> discoveries = new List<int>();

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
		Library = Library.Instance;
	}

	IEnumerator Start() {
		while (!Save.load)
			yield return null;

		var d = Save.discovered;
		discoveries = new List<int>(d);

		if (!Preset.persistDiscoveries) discoveries = new List<int>(); // Reset memories

		m_load = true;
	}

	public bool HasDiscoveredFile(string path)
	{
		int index = Library.ALL_FILES.IndexOf(path);
		if (index >= 0) 
		{
			return discoveries.Contains(index);
		}

		return false;
	}

	public bool DiscoverFile(string path)
	{
		if (HasDiscoveredFile(path)) return false;

		int index = Library.ALL_FILES.IndexOf(path);
		discoveries.Add(index);

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