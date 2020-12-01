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
using B83.Win32;
using uwu;
using uwu.Extensions;
using uwu.IO;
using Object = System.Object;

public class Library : Singleton<Library>
{
	public bool READ_IN_EDITOR_MODE = true;
	public Texture2D DEFAULT_NULL_TEXTURE;
	public Dictionary<string, Texture2D[]> TEXTURE_PACKS = new Dictionary<string, Texture2D[]>();

	// Events

	public System.Action OnRefreshItems;

	public System.Action<string, POINT> onAddFileInstance;
	public System.Action<string[]> onAddedFiles, onRemovedFiles;

	public System.Action<string> onDiscoverFile;

	// External

	FileNavigator Files = null;
	Quilt Quilt = null;
	GameDataSaveSystem Save;

	// Collections


	public List<string> ALL_FILES = new List<string>();
	public List<int> SHARED_FILES = new List<int>();
	public List<string> TEMP_FILES = new List<string>();

	Dictionary<string, List<string>> FILE_LOOKUP = new Dictionary<string, List<string>>();

	[SerializeField] List<string> textureQueue = new List<string>();
	[SerializeField] List<string> textureLoadTarget = new List<string>();

	[SerializeField] List<Texture2D> temp_textures = new List<Texture2D>();
	[SerializeField] Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

	// Properties

	WWW textureWWW;

	// Attributes

	[SerializeField] bool read = false, load = false, initialized = false;

	#region Accessors

	public float loadProgress 
	{
		get
		{
			if (initialized) {
				int files = textureLoadTarget.Count();
				if (files == 0)
					return 1f;
				
				int textures = (files - textureQueue.Count);
				return (float)textures / files;
			}

			return 0f;
		}
	}

	public string[] wizard_files => FILE_LOOKUP["WIZARD"].ToArray();
	public string[] desktop_files => FILE_LOOKUP["DESKTOP"].ToArray();
	public string[] shared_files => FILE_LOOKUP["SHARED"].ToArray();
	public string[] enviro_files => FILE_LOOKUP["ENVIRONMENT"].ToArray();

	#endregion

	#region Monobehaviour callbacks

	void OnDestroy()
	{
		Files.onRefresh -= RefreshFiles;
		Quilt.onLoadTexture -= AddTextureToLibrary;

		Dispose();
	}

	#endregion
	
	#region Initialization

	public void Load(string[] files, Texture2D NULL_TEXTURE, Dictionary<string, Texture2D[]> TEXTURE_PACKS, bool loadTexturesImmediate)
	{
		Save = GameDataSaveSystem.Instance;

		Files = FileNavigator.Instance;
			Files.onRefresh += RefreshFiles;

		Quilt = Quilt.Instance;
			Quilt.onLoadTexture += AddTextureToLibrary;

		READ_IN_EDITOR_MODE = loadTexturesImmediate;
		DEFAULT_NULL_TEXTURE = NULL_TEXTURE;
		this.TEXTURE_PACKS = TEXTURE_PACKS;
			
		ALL_FILES = new List<string>(files);
		TEMP_FILES = new List<string>(files);
		
		FILE_LOOKUP = new Dictionary<string, List<string>>() 
		{
			{ "WIZARD", new List<string>() },
			{ "DESKTOP", new List<string>() },
			{ "SHARED", new List<string>() },
			{ "ENVIRONMENT", new List<string>() },
		};
		
		Files.Refresh();

		initialized = true;
	}

	public void Reload()
	{
		Files.Refresh(); // Refresh all files
	}

	void Dispose()
	{
		if (load) {
			StopCoroutine("LoadFromQueue");
			load = false;
		}

		Texture2D[] textures = this.textures.Values.ToArray();
		for (int i = 0; i < textures.Length; i++) {
			var tex = textures[i];
			var name = tex.name;

			if(desktop_files.Contains(name)) // Destroy texture if loaded from desktop
				Destroy(textures[i]);
		}
		this.textures.Clear();
		textureQueue.Clear();

		FILE_LOOKUP.Clear();
		ALL_FILES.Clear();

		Texture2D[] temptextures = temp_textures.ToArray();
		for (int i = 0; i < temptextures.Length; i++) {
			Destroy(temptextures[i]);
		}

		if (textureWWW != null) {
			textureWWW.Dispose();
			textureWWW = null;
		}
	}
	
	#endregion

	#region Refreshing files

	void RefreshDesktopFiles()
    {
	    textureLoadTarget = new List<string>();
	    
		var paths = Files.GetPaths();
		for (int i = 0; i < paths.Length; i++) {
			var path = paths[i];
			
			RegisterFile(path, "DESKTOP");
			if (!textures.ContainsKey(path)) 
				textureLoadTarget.Add(path);
		}

#if !UNITY_EDITOR
		LoadTextures(textureLoadTarget.ToArray());
#else
		if (READ_IN_EDITOR_MODE)
			LoadTextures(textureLoadTarget.ToArray());
#endif
	}

	void RefreshTexturePacks()
	{
		foreach (KeyValuePair<string, Texture2D[]> PACK in TEXTURE_PACKS) {
			foreach (Texture2D tex in PACK.Value) {
				AddTextureFromPack(PACK.Key, tex);
			}
		}
		
		void AddTextureFromPack(string pack, Texture2D image)
		{
			var id = image.name;
			var tex = image;

			RegisterFile(id, pack);
			AddTextureToLibrary(id, tex);
		}
	}
	
	void RefreshSharedFiles()
	{
		if (Save == null) return;

		var indices = Save.shared_files;
		SHARED_FILES = new List<int>(indices);
		
		for (int i = 0; i < indices.Length; i++) {
			var index = indices[i];
			var file = ALL_FILES[index];
			
			RegisterFile(file, "SHARED");
		}
	}

	void RefreshFiles()
	{
		string[] CACHE_TEMP_FILES = TEMP_FILES.ToArray();
		TEMP_FILES = new List<string>(); // Wipe temp files alloc
		
		RefreshDesktopFiles();
		RefreshSharedFiles();
		RefreshTexturePacks();

		IEnumerable<string> ADDED_FILES = TEMP_FILES.Except(CACHE_TEMP_FILES);
		IEnumerable<string> REMOVED_FILES = CACHE_TEMP_FILES.Except(TEMP_FILES);

		// Fire events for file status changes
		if (onAddedFiles != null) 
			onAddedFiles(ADDED_FILES.ToArray());
		if (onRemovedFiles != null) 
			onRemovedFiles(REMOVED_FILES.ToArray());

		Save.files = ALL_FILES.ToArray();
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
	
	public Texture2D GetTexture(string path)
	{
		if (!textures.ContainsKey(path)) {
			return DEFAULT_NULL_TEXTURE;
		}
		return textures[path];
	}

	public void SaveTexture(string name, Texture2D image)
	{
		var root = Files.Path;
		var path = Path.Combine(root, string.Format("{0}.jpg", name));
		
		if (ALL_FILES.Contains(path)) return;

		var bytes = image.EncodeToJPG();
		File.WriteAllBytes(path, bytes);

		textures.Add(path, image);
		temp_textures.Add(image);

		RegisterFile(name, "SHARED");
		
		SHARED_FILES.Add(ALL_FILES.Count - 1);

		Save.files = ALL_FILES.ToArray();
		Save.shared_files = SHARED_FILES.ToArray();
	}

	#endregion
	
	#region Files
	
	public bool ContainsFile(string file)
	{
		return ALL_FILES.Contains(file);
	}

	public int FetchFileIndex(string filename)
	{
		return ALL_FILES.IndexOf(filename);
	}

	public bool FetchFile(int index, out string filename) 
	{
		if (index >= 0 && index < ALL_FILES.Count){
			filename = ALL_FILES[index];
			return true;
		}

		filename = null;
		return false;
	}

	public bool RegisterFileInstance(string file, POINT screenPoint)
	{
		bool success = RegisterFile(file, "DESKTOP");

		if (success && onAddFileInstance != null) 
			onAddFileInstance(file, screenPoint);

		return success;
	}

	bool RegisterFile(string file, string type)
	{
		bool exists = true;
		
		if (!ALL_FILES.Contains(file)) 
		{
			ALL_FILES.Add(file);
			exists = false;
		}

		if (!TEMP_FILES.Contains(file)) {
			TEMP_FILES.Add(file);
		}

		AddToFileLookup(type, file);

		if (!exists && (onDiscoverFile != null))
			onDiscoverFile(file); // Discover file on add file
		
		return exists;
	}

	void AddToFileLookup(string type, string file)
	{
		if (FILE_LOOKUP.ContainsKey(type)) {
			var list = FILE_LOOKUP[type];
			if (!list.Contains(file)) 
				list.Add(file);
		}
		else {
			FILE_LOOKUP[type] = new List<string>(new string[]{ file });
		}
	}

	#endregion
	
	#region Discovery
	
	public bool HasDiscoveredFile(string path)
	{
		int index = ALL_FILES.IndexOf(path);
		return (index >= 0);
	}
	
	#endregion

	#region File types
		
		public bool IsDesktop(Beacon beacon) { return (beacon == null) ? false : IsDesktop(beacon.file); }
		public bool IsWizard(Beacon beacon) { return (beacon == null) ? false : IsWizard(beacon.file); }
		public bool IsShared(Beacon beacon) { return (beacon == null) ? false : IsShared(beacon.file); }
		public bool IsEnviro(Beacon beacon) { return (beacon == null) ? false : IsEnviro(beacon.file); }

		public bool IsDesktop(string file)
		{
			return FILE_LOOKUP.ContainsKey("DESKTOP") && desktop_files.Contains(file);
		}

		public bool IsWizard(string file)
		{
			return FILE_LOOKUP.ContainsKey("WIZARD") && wizard_files.Contains(file);
		}

		public bool IsShared(string file)
		{
			return FILE_LOOKUP.ContainsKey("SHARED") && shared_files.Contains(file);
		}

		public bool IsEnviro(string file)
		{
			return FILE_LOOKUP.ContainsKey("ENVIRONMENT") && enviro_files.Contains(file);
		}
		
	#endregion
}
