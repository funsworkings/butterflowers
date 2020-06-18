using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using System.Threading.Tasks;

using Memories = Wizard.Memories;
using UnityEditor;
using System.Security.Cryptography;

public class Library : Singleton<Library>
{
	#region Events

	public System.Action OnRefreshItems;
	public System.Action<string> OnAddItem;

	#endregion

	#region External

	FileNavigator Files = null;

	Memories m_WizardFiles = null;
	public Memories WizardFiles {
		get { return m_WizardFiles; }
		set {
			bool refresh = (m_WizardFiles != value);

			m_WizardFiles = value;
			if (refresh) RefreshWizardFiles();
		} 
	}

	Quilt Quilt = null;
	GameDataSaveSystem Save;

	#endregion

	#region Collections

	[SerializeField] List<string> items = new List<string>();
	[SerializeField] List<string> items_wizard = new List<string>();
	[SerializeField] List<string> items_desktop = new List<string>();
	[SerializeField] List<string> items_shared = new List<string>();

	[SerializeField] List<string> textureQueue = new List<string>();
	[SerializeField] List<Texture2D> temp_textures = new List<Texture2D>();
	[SerializeField] Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

	#endregion

	#region Properties

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

	public string[] wizard_files {
		get
		{
			return items_wizard.ToArray();
		}
	}

	public string[] desktop_files {
		get
		{
			return items_desktop.ToArray();
		}
	}

	public string[] shared_files {
		get
		{
			return items_shared.ToArray();
		}
	}

	#endregion

	#region Monobehaviour callbacks

	void OnDestroy()
	{
		Files.onRefresh -= RefreshFiles;
		Quilt.onLoadTexture -= AddTextureToLibrary;

		Dispose();
	}

	#endregion

	#region Internal

	void RefreshDesktopFiles()
    {
		items_desktop = new List<string>(Files.GetPaths());

#if !UNITY_EDITOR
		LoadTextures(items_desktop.ToArray());
#endif
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
	}

	void RefreshSharedFiles()
	{
		if (Save == null) return;
		items_shared = new List<string>(Save.shared_files);
	}

	void RefreshFiles()
	{
		Dispose();

		RefreshDesktopFiles();
		RefreshWizardFiles();
		RefreshSharedFiles();

		items = (items_desktop.Concat(items_wizard).Concat(items_shared)).ToList();
		if (OnRefreshItems != null)
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
		Files.onRefresh += RefreshFiles;

		Quilt = Quilt.Instance;
		Quilt.onLoadTexture += AddTextureToLibrary;

		Save = GameDataSaveSystem.Instance;

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
			var tex = textures[i];
			var name = tex.name;

			if(items_desktop.Contains(name)) // Destroy texture if loaded from desktop
				Destroy(textures[i]);
		}
		this.textures.Clear();
		textureQueue.Clear();

		items_desktop.Clear();
		items_wizard.Clear();
		items_shared.Clear();
		items.Clear();

		Texture2D[] temptextures = temp_textures.ToArray();
		for (int i = 0; i < temptextures.Length; i++) {
			Destroy(temptextures[i]);
		}

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

	public void SaveTexture(string name, Texture2D image)
	{
		var root = Files.Path;
		var path = Path.Combine(root, string.Format("{0}.jpg", name));

		if (items.Contains(path)) return;

		var bytes = image.EncodeToJPG();
		File.WriteAllBytes(path, bytes);

		items.Add(path);
		items_shared.Add(path);

		textures.Add(path, image);
		temp_textures.Add(image);

		if (OnAddItem != null)
			OnAddItem(path);

		Save.shared_files = items_shared.ToArray();
	}

	public bool ContainsFile(string file)
	{
		return items.Contains(file);
	}

	public bool IsDesktop(Beacon beacon) { return (beacon == null) ? false : IsDesktop(beacon.file); }
	public bool IsWizard(Beacon beacon) { return (beacon == null) ? false : IsWizard(beacon.file); }
	public bool IsShared(Beacon beacon) { return (beacon == null) ? false : IsShared(beacon.file); }

	public bool IsDesktop(string file)
	{
		return items_desktop.Contains(file);
	}

	public bool IsWizard(string file)
	{
		return items_wizard.Contains(file);
	}

	public bool IsShared(string file)
	{
		return items_shared.Contains(file);
	}

#endregion
}
