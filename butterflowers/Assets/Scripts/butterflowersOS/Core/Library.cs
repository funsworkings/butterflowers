using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using B83.Win32;
using butterflowersOS.Data;
using UnityEngine;
using uwu.Extensions;
using uwu.IO;
using uwu.Snippets.Load;
using uwu.Textures;

namespace butterflowersOS.Core
{
	public class Library : Singleton<Library>, ITextureReceiver, ILoadDependent
	{
		public bool READ_IN_EDITOR_MODE = true;
		public Texture2D DEFAULT_NULL_TEXTURE;
		public Dictionary<string, Texture2D[]> TEXTURE_PACKS = new Dictionary<string, Texture2D[]>();
	
	
		#region Internal

		public enum FileType
		{
			World,
			User,
			Shared
		}
	
		#endregion

		// Events

		public System.Action OnRefreshItems;

		public System.Action<string, POINT> onAddFileInstance;
		public System.Action<string[]> onAddedFiles, onRemovedFiles, onDeletedFiles, onRecoverFiles;

		public System.Action<string> onDiscoverFile;

		// External

		FileNavigator Files = null;
		TextureLoader TextureLoader = null;

		// Collections
	
		public List<string> ALL_FILES = new List<string>();
		public List<string> ALL_DIRECTORIES = new List<string>();
		public Dictionary<string, bool> TEMP_FILES = new Dictionary<string, bool>();
	
		public List<int> USER_FILES = new List<int>();
		public List<int> SHARED_FILES = new List<int>();
		public List<int> WORLD_FILES = new List<int>();

		Dictionary<FileType, List<string>> FILE_LOOKUP = new Dictionary<FileType, List<string>>();
		Dictionary<string, Texture2D> TEXTURE_LOOKUP = new Dictionary<string, Texture2D>();

		[SerializeField] List<string> textureQueue = new List<string>();
		[SerializeField] List<string> textureLoadTarget = new List<string>();

		// Properties

		WWW textureWWW;

		// Attributes

		[SerializeField] bool read = false, load = false, initialized = false;
		[SerializeField] bool listenForEvents = false;

		#region Accessors

		public float loadProgress 
		{
			get
			{
				if (initialized) 
				{
					int files = textureLoadTarget.Count();
					if (files == 0)
						return 1f;
				
					int textures = (files - textureQueue.Count);
					return (float)textures / files;
				}

				return 0f;
			}
		}

		public float Progress { get => loadProgress; }
		public bool Completed { get => loadProgress >= 1f; }

		public string[] UserFiles => FILE_LOOKUP[FileType.User].ToArray();
		public string[] SharedFiles => FILE_LOOKUP[FileType.Shared].ToArray();
		public string[] WorldFiles => FILE_LOOKUP[FileType.World].ToArray();

		#endregion

		#region Monobehaviour callbacks

		void Start()
		{
			TextureLoader = TextureLoader.Instance;
		}

		void OnDestroy()
		{
			Dispose();
		}

		void OnApplicationFocus(bool hasFocus)
		{
			if(hasFocus) StopListen();
			else StartListen();
		}

		void OnApplicationPause(bool pauseStatus)
		{
			if(pauseStatus) StartListen();
			else StopListen();
		}

		#endregion
	
		#region Initialization

		public object Save()
		{
			var payload = new LibraryPayload();
			payload.directories = ALL_DIRECTORIES.ToArray();
			payload.files = ALL_FILES.ToArray();
			payload.userFiles = USER_FILES.ToArray();
			payload.sharedFiles = SHARED_FILES.ToArray();
			payload.worldFiles = WORLD_FILES.ToArray();
		
			return payload;
		}

		public void Load(LibraryPayload payload, Texture2D NULL_TEXTURE, Dictionary<string, Texture2D[]> TEXTURE_PACKS, bool loadTexturesImmediate)
		{
			Files = FileNavigator.Instance;

			READ_IN_EDITOR_MODE = loadTexturesImmediate;
			DEFAULT_NULL_TEXTURE = NULL_TEXTURE;
			this.TEXTURE_PACKS = TEXTURE_PACKS;
			
			ALL_DIRECTORIES = new List<string>(payload.directories);
			ALL_FILES = new List<string>(payload.files);
		
			USER_FILES = new List<int>(payload.userFiles);
			SHARED_FILES = new List<int>(payload.sharedFiles);
			WORLD_FILES = new List<int>(payload.worldFiles);
		
			FILE_LOOKUP = new Dictionary<FileType, List<string>>() 
			{
				{ FileType.World, new List<string>() },
				{ FileType.User, new List<string>() },
				{ FileType.Shared, new List<string>() }
			};

			Restore();

			if (textureLoadTarget.Count > 0) // Load all necessary textures
			{
				// Load all textures from restore
				#if !UNITY_EDITOR
					LoadTextures(textureLoadTarget.ToArray());
				#else
				
				if (READ_IN_EDITOR_MODE) LoadTextures(textureLoadTarget.ToArray());
				
				#endif
			}

			initialized = true;
		}

		public void Aggregate(LibraryPayload payload)
		{
			var files = payload.files;

			var user_files = payload.userFiles;
			var shared_files = payload.sharedFiles;
			var world_files = payload.worldFiles;

			foreach (int uf in user_files) RegisterFile(files[uf], FileType.User);
			foreach (int sf in shared_files) RegisterFile(files[sf], FileType.Shared);
			foreach (int wf in world_files) RegisterFile(files[wf], FileType.World);
		}

		void Restore()
		{
			foreach (KeyValuePair<FileType, List<string>> file in FILE_LOOKUP) 
			{
				var filetype = file.Key;
			
				var lookup = new List<int>();
				if (filetype == FileType.User) lookup = USER_FILES;
				if (filetype == FileType.Shared) lookup = SHARED_FILES;
				if (filetype == FileType.World) lookup = WORLD_FILES;

				var files = ALL_FILES.TakeWhile((_file, index) => lookup.Contains(index)).ToArray();
				foreach (string f in files) 
				{
					RegisterFile(f, filetype, load:true);
				}
			}
		}

		void Dispose()
		{
			if (load) {
				StopCoroutine("LoadFromQueue");
				load = false;
			}

			Texture2D[] textures = this.TEXTURE_LOOKUP.Values.ToArray();
			for (int i = 0; i < textures.Length; i++) {
				var tex = textures[i];
				var name = tex.name;

				if(UserFiles.Contains(name)) // Destroy texture if loaded from desktop
					Destroy(textures[i]);
			}
			this.TEXTURE_LOOKUP.Clear();
			textureQueue.Clear();

			FILE_LOOKUP.Clear();
			ALL_FILES.Clear();

			if (textureWWW != null) {
				textureWWW.Dispose();
				textureWWW = null;
			}
		}
	
		#endregion

		#region Textures

		void LoadTextures(string[] paths)
		{
			textureQueue = new List<string>(paths);

			if (!load) 
			{
				load = true;
				StartCoroutine("LoadFromQueue");
			}
		}

		IEnumerator LoadFromQueue()
		{
			while (textureQueue.Count > 0) 
			{
				if (!read) 
				{
					string file = textureQueue[0];
					TextureLoader.Push(file, this);
				
					read = true;
				}

				yield return null;
			}

			load = false;
		}

		public void ReceiveTexture(string file, Texture2D texture)
		{
			RegisterTexture(file, texture);

			if (load) 
			{
				textureQueue.Remove(file);
				read = false;
			}
		}

		#endregion
	
		#region Lookup
	
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
			if (index >= 0 && index < ALL_FILES.Count)
			{
				filename = ALL_FILES[index];
				return true;
			}

			filename = null;
			return false;
		}
	
		void AddToFileLookup(FileType type, string file)
		{
			if (FILE_LOOKUP.ContainsKey(type)) 
			{
				var list = FILE_LOOKUP[type];
				if (!list.Contains(file)) 
					list.Add(file);
			}
			else 
			{
				FILE_LOOKUP[type] = new List<string>(new string[]{ file });
			}
		}
	
		public Texture2D GetTexture(string path, bool nullable = false)
		{
			if (!TEXTURE_LOOKUP.ContainsKey(path)) 
				return (nullable)? null:DEFAULT_NULL_TEXTURE;
		
			return TEXTURE_LOOKUP[path];
		}

		public Texture2D GetWorldTexture(string id)
		{
			foreach (KeyValuePair<string, Texture2D[]> pack in TEXTURE_PACKS) 
			{
				foreach (Texture2D tex in pack.Value) 
				{
					var textureID = tex.name;
					if (textureID == id) return tex;
				}
			}

			return DEFAULT_NULL_TEXTURE;
		}
	
		#endregion
	
		#region Register
	
		public string CreateFile(string name, Texture2D image)
		{
			var root = Files.Path;
			var path = Path.Combine(root, string.Format("{0}.jpg", name));
		
			try 
			{
				var bytes = image.EncodeToJPG();
				File.WriteAllBytes(path, bytes);
			
				RegisterFile(name, FileType.Shared);
				RegisterTexture(path, image);
			}
			catch (System.Exception e) 
			{
				Debug.LogWarning("Unable to create file! " + e.Message);
			}

			return path;
		}

		public bool RegisterFileInstance(string file, POINT screenPoint)
		{
			bool success = RegisterFile(file, FileType.User);

			if (success && onAddFileInstance != null) 
				onAddFileInstance(file, screenPoint);

			return success;
		}

		public bool RegisterFile(string file, FileType type, string directory = null, bool load = false)
		{
			bool success = true;
			bool @new = !ALL_FILES.Contains(file); // New entry to files

			if (type == FileType.User || type == FileType.Shared)  // Verify file exists (user or shared)
			{
				var fileInfo = new FileInfo(file);
				success = fileInfo.Exists;

				if (success)
					directory = fileInfo.Directory.FullName;
			}

			if (@new) 
			{
				ALL_FILES.Add(file);
				if(directory != null && !ALL_DIRECTORIES.Contains(directory)) ALL_DIRECTORIES.Add(directory);

				int index = ALL_FILES.Count - 1;
			
				if(type == FileType.User) USER_FILES.Add(index);
				else if(type == FileType.Shared) SHARED_FILES.Add(index);
				else if(type == FileType.World) WORLD_FILES.Add(index);

				if(onDiscoverFile != null)
					onDiscoverFile(file); // Discover file on add file

				if (OnRefreshItems != null)
					OnRefreshItems();
			}
		
			AddToFileLookup(type, file);

			// Add to textures
			if (load) 
			{
				if (success) 
				{
					if (type == FileType.World) RegisterTexture(file, GetWorldTexture(file));
					else textureLoadTarget.Add(file);
				}
				else 
					RegisterTexture(file, DEFAULT_NULL_TEXTURE); // Fallback to NULL texture
			}

			return success;
		}

		public bool UnregisterFile(string file)
		{
			return false;
		}

		public void RegisterTexture(string file, Texture tex)
		{
			var texture = (tex as Texture2D);
			if (texture == null) texture = DEFAULT_NULL_TEXTURE;

			if (TEXTURE_LOOKUP.ContainsKey(file)) TEXTURE_LOOKUP[file] = texture;
			else TEXTURE_LOOKUP.Add(file, texture);

			Debug.LogFormat("Added {0} to library", file);
		}

		#endregion
	
		#region Deprecation

		void StartListen()
		{
			if (!listenForEvents) 
			{
				TEMP_FILES.Clear();
				foreach (string file in ALL_FILES) 
				{
					var fileInfo = new FileInfo(file);
					var exists = fileInfo.Exists;

					if (exists) TEMP_FILES.Add(file, true);
					else TEMP_FILES.Add(file, false);
				}
			
				StartCoroutine(ListenForFileEvents()); 
				listenForEvents = true;
			}
		}
		void StopListen(){ if(listenForEvents){ StopCoroutine(ListenForFileEvents()); listenForEvents = false; } }

		IEnumerator ListenForFileEvents()
		{
			List<string> deprecate = new List<string>();
			List<string> recover = new List<string>();

			while (true) 
			{
				foreach (string file in ALL_FILES) 
				{
					var fileInfo = new FileInfo(file);
					var exists = fileInfo.Exists;
				
					var cacheExists = false;
					if (!TEMP_FILES.TryGetValue(file, out cacheExists)) 
					{
						TEMP_FILES.Add(file, exists);
						cacheExists = exists;
					}

					if (cacheExists != exists) {
						if (exists) recover.Add(file);
						else deprecate.Add(file);

						TEMP_FILES[file] = exists;
					}
				}
			
#if !UNITY_EDITOR
			Debug.LogErrorFormat("User deleted {0} files\nUser recovered {1} files", deprecate.Count, recover.Count);
#else
				Debug.LogWarningFormat("User deleted {0} files\nUser recovered {1} files", deprecate.Count, recover.Count);
#endif

				if (deprecate.Count > 0 && onDeletedFiles != null)
					onDeletedFiles(deprecate.ToArray());

				if (recover.Count > 0 && onRecoverFiles != null)
					onRecoverFiles(recover.ToArray());
			
				deprecate.Clear();
				recover.Clear();

				yield return new WaitForSecondsRealtime(1f);
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
	
		public bool IsUserFile(string file) => FILE_LOOKUP[FileType.User].Contains(file);
		public bool IsSharedFile(string file) => FILE_LOOKUP[FileType.Shared].Contains(file);
		public bool IsWorldFile(string file) => FILE_LOOKUP[FileType.World].Contains(file);

		#endregion
	}
}
