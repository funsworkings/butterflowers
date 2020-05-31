using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

using MemoryBank = Wizard.MemoryBank;

public class Library : Singleton<Library>
{
	#region Events

	public System.Action OnRefreshItems;

	#endregion

	#region External

	FileNavigator Files = null;

	MemoryBank m_WizardFiles = null;
	public MemoryBank WizardFiles {
		get { return m_WizardFiles; }
		set {
			bool refresh = (m_WizardFiles != value);

			m_WizardFiles = value;
			if (refresh) RefreshWizardFiles();
		} 
	}

	#endregion

	#region Collections

	[SerializeField] List<string> items = new List<string>();

	[SerializeField] List<string> items_wizard = new List<string>();
	[SerializeField] List<string> items_desktop = new List<string>();
	[SerializeField] List<string> items_external = new List<string>();

	#endregion

	#region Monobehaviour callbacks

	void OnDestroy()
	{
		Files.onRefresh -= RefreshDesktopFilesWithEvents;
	}

	#endregion

	#region Internal

	void RefreshDesktopFilesWithEvents() { RefreshDesktopFiles(true); }

	void RefreshDesktopFiles(bool events = true)
    {
		items_desktop = new List<string>(Files.GetPaths());

		RefreshFiles(events);
    }

	void RefreshWizardFiles()
	{
		if (WizardFiles == null) 
		{
			items_wizard = new List<string>();
			return;
		}

		var memories = WizardFiles.items.Where(memory => !string.IsNullOrEmpty(memory.name));
		items_wizard = new List<string>(memories.Select(memory => memory.name)); // Set items list from memory names

		RefreshFiles();
	}

	void RefreshFiles(bool events = false)
	{
		items = (items_desktop.Concat(items_wizard)).ToList();
		if (OnRefreshItems != null && events)
			OnRefreshItems();
	}

	#endregion

	#region Operations

	public void Initialize(bool refresh = true)
	{
		if (Files != null) return;

		Files = FileNavigator.Instance;
		Files.onRefresh += RefreshDesktopFilesWithEvents;

		if (refresh)
			Files.Refresh();
		else
			RefreshDesktopFiles(false);
	}

	public bool ContainsFile(string file)
	{
		return items.Contains(file);
	}

	public bool IsDesktop(string file)
	{
		return items_desktop.Contains(file);
	}

	public bool IsWizard(string file)
	{
		return items_wizard.Contains(file);
	}

	#endregion
}
