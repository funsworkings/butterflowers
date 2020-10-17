using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using uwu;
using uwu.Extensions;

public class Discoveries: Singleton<Discoveries> {

	// Events

	public static System.Action<string> onDiscover;

	// External

	Library Library;

	// Collections

	public List<int> discoveries = new List<int>();

	
	#region Initialize

	public void Load(int[] discoveries, bool persist)
	{
		Library = Library.Instance;
		
		this.discoveries = new List<int>(discoveries);

		if (!persist) 
			this.discoveries.Clear();
	}
	
	#endregion
	
	#region Discoveries

	public bool HasDiscoveredFile(string path)
	{
		int index = Library.ALL_FILES.IndexOf(path);
		if (index >= 0) 
		{
			return discoveries.Contains(index);
		}

		return false;
	}

	/// <summary>
	/// Attempt to discover a new file from file system
	/// </summary>
	/// <param name="path">Path of file</param>
	/// <returns>Has discovered file before</returns>
	public bool DiscoverFile(string path)
	{
		if (HasDiscoveredFile(path)) return false;

		int index = Library.ALL_FILES.IndexOf(path);
		discoveries.Add(index);

		if (onDiscover != null)
			onDiscover(path);

		return true;
	}

	#endregion
}