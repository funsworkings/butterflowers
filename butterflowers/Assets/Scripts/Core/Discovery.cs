using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;
using System.Linq;

public class Discovery: Singleton<Discovery> {

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

		if (!Preset.memory) discoveries = new List<string>(); // Reset memories

		m_load = true;
	}

	public bool HasDiscoveredFile(string path)
	{
		return discoveries.Contains(path);
	}

	public void DiscoverFile(string path)
	{
		if (HasDiscoveredFile(path)) return;

		discoveries.Add(path);
		SendDiscoveries();
	}

	void SendDiscoveries()
	{
		Save.discovered = discoveries.ToArray();
	}
}