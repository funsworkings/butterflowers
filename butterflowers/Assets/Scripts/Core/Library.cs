using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using System.Threading.Tasks;

using MemoryBank = Wizard.MemoryBank;
using UnityEditor;
using System.Security.Cryptography;

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

	Quilt Quilt = null;

	#endregion

	#region Collections

	[SerializeField] List<string> items = new List<string>();

	[SerializeField] List<string> items_wizard = new List<string>();
	[SerializeField] List<string> items_desktop = new List<string>();
	[SerializeField] List<string> items_external = new List<string>();

	[SerializeField] List<string> textureQueue = new List<string>();
	[SerializeField] Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
	WWW textureWWW;

	#endregion

	#region Attributes

	[SerializeField] bool read = false, load = false, initialized = false;

	#endregion

	#region Accessors

	public float loadprogress 
	{
		get
		{
			if (initialized)
			{
				int files = items_desktop.Count;
				int textures = (files - textureQueue.Count);

				return (float)textures / files;
			}

			return 0f;
		}
	}

	#endregion

	#region Monobehaviour callbacks

	void OnDestroy()
	{
		Files.onRefresh -= RefreshDesktopFilesWithEvents;
		Quilt.onLoadTexture -= AddTextureToLibrary;

		Dispose();
	}

	#endregion

	#region Internal

	void RefreshDesktopFilesWithEvents() { RefreshDesktopFiles(true); }

	void RefreshDesktopFiles(bool events = true)
    {
		items_desktop = new List<string>(Files.GetPaths());
		
		LoadTextures(items_desktop.ToArray());
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

		// Add wizard memory images to texture lookup
		for (int i = 0; i < memories.Count(); i++) {
			var mem = memories.ElementAt(i);
			AddTextureToLibrary(mem.name, mem.image);
		}

		RefreshFiles();
	}

	void RefreshFiles(bool events = false)
	{
		items = (items_desktop.Concat(items_wizard)).ToList();
		if (OnRefreshItems != null && events)
			OnRefreshItems();
	}

	#endregion

	#region Textures

	void LoadTextures(string[] paths)
	{
		textureQueue = new List<string>(paths);

		if (!load) {
			load = true;
			StartCoroutine("LoadFromQueue");
		}
	}

	IEnumerator LoadFromQueue()
	{
		while (textureQueue.Count > 0) {
			if (!read) {
				string file = textureQueue[0];
				textureQueue.RemoveAt(0);

				if (!textures.ContainsKey(file)) {
					read = true;
					ReadBytes(file);
				}
			}

			yield return null;
		}

		load = false;
	}

	async Task ReadBytes(string file)
	{
		var www = textureWWW = new WWW(string.Format("file://{0}", file));
		var _www = UnityWebRequestTexture.GetTexture(string.Format("file://{0}", file));

		_www.SendWebRequest();

		while (!_www.isDone)
			await Task.Delay(100);

		read = false;
		if (_www.isNetworkError || _www.isHttpError) {
			throw new System.Exception("Error reading from texture --> NETWORK");
		}

		Debug.Log("Success load = " + file);
		var texture = DownloadHandlerTexture.GetContent(_www);
		texture.name = file;

		//textureWWW.Dispose();
		//textureWWW = null;

		_www.Dispose();

		AddTextureToLibrary(file, texture);
	}

	void AddTextureToLibrary(string file, Texture tex)
	{
		var texture = (tex as Texture2D);

		if (textures.ContainsKey(file)) textures[file] = texture;
		else textures.Add(file, texture);

		Debug.LogFormat("Added {0} to library", file);
	}

	#endregion

	#region Operations

	public void Initialize(bool refresh = true)
	{
		if (Files != null || initialized) return;

		Files = FileNavigator.Instance;
		Files.onRefresh += RefreshDesktopFilesWithEvents;

		Quilt = Quilt.Instance;
		Quilt.onLoadTexture += AddTextureToLibrary;

		Files.Refresh();
		initialized = true;
	}

	public void Dispose()
	{
		if (load) {
			StopCoroutine("LoadFromQueue");
			load = false;
		}

		Texture2D[] textures = this.textures.Values.ToArray();
		for (int i = 0; i < textures.Length; i++) {
			Destroy(textures[i]);
		}
		this.textures.Clear();

		if (textureWWW != null) {
			textureWWW.Dispose();
			textureWWW = null;
		}
	}

	public Texture2D GetTexture(string path)
	{
		if (!textures.ContainsKey(path)) return null;
		return textures[path];
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
