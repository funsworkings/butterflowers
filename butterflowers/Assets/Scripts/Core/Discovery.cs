using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;
using System.Linq;

public class Discovery: Singleton<Discovery> {

	#region Events

	public System.Action onLoad;

	#endregion

	#region External

	GameDataSaveSystem Save;
	public Settings.WorldPreset Preset = null;

	#endregion

	#region Collections

	[SerializeField] List<string> discoveries = new List<string>();

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

		if (onLoad != null)
			onLoad();
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