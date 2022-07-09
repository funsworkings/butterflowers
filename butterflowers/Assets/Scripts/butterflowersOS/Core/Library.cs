using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using B83.Win32;
using butterflowersOS.Data;
using UnityEngine;
using uwu.Extensions;
using uwu.IO;
using uwu.IO.SimpleFileBrowser.Scripts;
using uwu.Snippets.Load;
using uwu.Textures;

namespace butterflowersOS.Core
{
	public class Library : MonoBehaviour, ITextureReceiver, ILoadDependent
	{
		public static Library Instance = null;
		
		
		public bool READ_IN_EDITOR_MODE = true;
		public bool CREATE_THUMBNAILS = true;
		
		public Texture2D DEFAULT_NULL_TEXTURE;
		public Dictionary<string, Texture2D[]> TEXTURE_PACKS = new Dictionary<string, Texture2D[]>();
	
	
		#region Internal

		public enum FileType
		{
			World,
			User,
			Shared
		}

		public enum LoadMode
		{
			NULL,
			
			Thumbnails,
			Files,
			Generate
		}

		#endregion

		// Events

		public System.Action OnRefreshItems;

		public System.Action<string, POINT> onAddFileInstance;
		public System.Action<string[]> onAddedFiles, onRemovedFiles, onDeletedFiles, onRecoverFiles;

		public System.Action<string> onDiscoverFile;

		// External

		FileNavigator Files = null;
		[SerializeField] TextureLoader TextureLoader = null;

		// Collections
	
		public List<string> ALL_FILES = new List<string>();
		public List<string> ALL_DIRECTORIES = new List<string>();

		public List<int> USER_FILES = new List<int>();
		public List<int> SHARED_FILES = new List<int>();
		public List<int> WORLD_FILES = new List<int>();

		Dictionary<FileType, List<string>> FILE_LOOKUP = new Dictionary<FileType, List<string>>();
		
		Dictionary<string, Texture2D> TEXTURE_LOOKUP = new Dictionary<string, Texture2D>();
		Dictionary<string, Texture2D> FALLBACK_TEXTURE_LOOKUP = new Dictionary<string, Texture2D>();
		Dictionary<string, ITextureReceiver> TEXTURE_RECEIVERS = new Dictionary<string, ITextureReceiver>();

		[SerializeField] List<string> thumbnailQueue = new List<string>();
		[SerializeField] List<string> generateThumbnailQueue = new List<string>();
		[SerializeField] List<string> fileQueue = new List<string>();
		
		[SerializeField] List<string> queue = new List<string>();
		[SerializeField] List<string> textureLoadCompleted = new List<string>();
		[SerializeField] List<string> textureLoadTarget = new List<string>();

		[SerializeField] Texture2D textureSheet = null;

		// Attributes

		[SerializeField] bool read = false, load = false, initialized = false;

		[SerializeField] LoadMode loadMode;
		[SerializeField] bool exportSheet = false;

		#region Accessors

		public float loadProgress 
		{
			get
			{
				if (initialized) 
				{
					int files = textureLoadTarget.Count();
					if (files == 0) return 1f;

					int textures = textureLoadCompleted.Count;
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

		public Texture2D[] Thumbnails => FALLBACK_TEXTURE_LOOKUP.Values.ToArray();
		public Texture2D TextureSheet => textureSheet;

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			if (Instance == null) 
			{
				Instance = this;
			}
			else 
			{
				Destroy(gameObject);	
			}
		} 

		void Update()
		{
			if (exportSheet) 
			{
				if (!load) 
				{
					ExportSheet("testSheet", out int _r, out int _c, out Texture2D tex);
					Destroy(tex);
				}

				exportSheet = false;
			}
		}

		void OnDestroy()
		{
			Dispose();

			if (Instance == this) 
			{
				Instance = null;
			}
		}

		void OnApplicationFocus(bool hasFocus)
		{
			return;
			
			//if(hasFocus) StopListen();
			//else StartListen();
		}

		void OnApplicationPause(bool pauseStatus)
		{
			return;
			
			//if(pauseStatus) StartListen();
			//else StopListen();
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

		public void Load(LibraryPayload payload, Texture2D NULL_TEXTURE, Dictionary<string, Texture2D[]> TEXTURE_PACKS, bool loadTexturesImmediate, bool createThumbnails, bool loadThumbnails, bool generateThumbnails)
		{
			Files = FileNavigator.Instance;

			READ_IN_EDITOR_MODE = loadTexturesImmediate;
			CREATE_THUMBNAILS = createThumbnails;
			
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
			
			if(loadThumbnails) LoadThumbnails();
			//if(generateThumbnails) GenerateThumbnails();
			//LoadFiles();

			textureLoadTarget = generateThumbnailQueue.Union(thumbnailQueue).ToList();
			//textureLoadTarget = textureLoadTarget.Union(fileQueue).ToList();
			
			if (textureLoadTarget.Count > 0) 
			{
				//if (generateThumbnailQueue.Count > 0) loadMode = LoadMode.Generate;
				if (thumbnailQueue.Count > 0) loadMode = LoadMode.Thumbnails;
				//else if (fileQueue.Count > 0) loadMode = LoadMode.Files;
				
				StartCoroutine("LoadingAllFiles");
			}
			else 
			{
				loadMode = LoadMode.NULL;
			}
			
			
			initialized = true;
			load = (loadMode != LoadMode.NULL);
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

				foreach (int index in lookup) 
				{
					var _file = ALL_FILES[index];
					RegisterFile(_file, filetype);
				}
			}
		}

		void Dispose()
		{
			if (load) 
			{
				StopCoroutine("LoadingAllFiles");
				load = false;
			}

			Texture2D[] textures = this.TEXTURE_LOOKUP.Values.ToArray();
			for (int i = 0; i < textures.Length; i++) 
			{
				var tex = textures[i];
				var name = tex.name;

				if(UserFiles.Contains(name)) // Destroy texture if loaded from desktop
					Destroy(textures[i]);
			}

			Texture2D[] thumbnails = FALLBACK_TEXTURE_LOOKUP.Values.ToArray();
			for(int i = 0; i < thumbnails.Length; i++)
			{
				Destroy(thumbnails[i]);
			}
			
			TEXTURE_LOOKUP.Clear();
			FALLBACK_TEXTURE_LOOKUP.Clear();
			thumbnailQueue.Clear();

			FILE_LOOKUP.Clear();
			ALL_FILES.Clear(); 
			
			if(textureSheet != null) Destroy(textureSheet);
		}
	
		#endregion
		
		#region Neueagent

		public bool AggregateNeueAgentData(LibraryPayload payload)
		{
			return true;
			
			/*
			var files = payload.files;

			var user_files = payload.userFiles;
			var shared_files = payload.sharedFiles;
			var world_files = payload.worldFiles;

			foreach (int uf in user_files) RegisterFile(files[uf], FileType.User);
			foreach (int sf in shared_files) RegisterFile(files[sf], FileType.Shared);
			foreach (int wf in world_files) RegisterFile(files[wf], FileType.World);
			
			return true;
			*/
		}
		
		#endregion

		#region Textures

		void LoadFiles()
		{
			var userFiles = FILE_LOOKUP[FileType.User];
			var sharedFiles = FILE_LOOKUP[FileType.Shared];

			fileQueue.AddRange(userFiles);
			fileQueue.AddRange(sharedFiles);
		}

		void LoadThumbnails()
		{
			var _directory = ThumbnailDirectory;
			if (Directory.Exists(_directory)) 
			{
				List<string> thumbnails = new List<string>();
				var files = Files.GetFiles(_directory);
				
				foreach(FileSystemEntry entry in files) 
				{
					var file = entry.Path;
					thumbnails.Add(file);
				}

				if (thumbnails.Count > 0) 
				{
					thumbnailQueue.AddRange(thumbnails);
				}
			}
		}

		void GenerateThumbnails()
		{
			foreach (KeyValuePair<string, Texture2D[]> texturePack in TEXTURE_PACKS) 
			{
				string collection = texturePack.Key;
				Texture2D[] pack = texturePack.Value;

				foreach (Texture2D texture in pack) 
				{
					string path = DefaultThumbnailPathFromFile(texture.name+".jpg");
					if (!thumbnailQueue.Contains(path)) 
					{
						generateThumbnailQueue.Add(path);
					}
				}
			}
		}

		IEnumerator LoadingAllFiles()
		{
			queue.AddRange(generateThumbnailQueue);
			queue.AddRange(thumbnailQueue);
			//queue.AddRange(fileQueue);

			do 
			{
				if (!read) 
				{
					if (loadMode == LoadMode.Generate) 
					{
						if (generateThumbnailQueue.Count <= 0) loadMode = LoadMode.Thumbnails;
					}
					
					if (loadMode == LoadMode.Thumbnails) 
					{
						if (thumbnailQueue.Count <= 0) loadMode = LoadMode.NULL;
					}

					if (loadMode == LoadMode.Files) 
					{
						if (fileQueue.Count <= 0) loadMode = LoadMode.NULL;
					}

					if (queue.Count > 0) 
					{
						string file = queue[0];

						if (loadMode != LoadMode.Generate) 
						{
							TextureLoader.Push(file, this);
							read = true;
						}
						else 
						{
							var _filename = Path.GetFileName(file);
							Texture2D texture = GetWorldTexture(_filename);
							
							if (texture != null) 
							{
								Debug.Log("Degrade " + _filename);
								
								Texture2D thumbnail = DegradeBytes(_filename, texture, false);
								
								RegisterTexture(_filename, null);
								RegisterThumbnail(file, thumbnail);
							}
							
							generateThumbnailQueue.RemoveAt(0);
							queue.RemoveAt(0);
							textureLoadCompleted.Add(file);
						}
					}
				}

				yield return null;
				
			} while (queue.Count > 0);

			load = false;
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

		#endregion
	
		#region Register
	
		public string CreateFile(string filename, string directory, Texture2D image, FileType type)
		{
			var fullpath = Path.Combine(directory, string.Format("{0}.jpg", filename));
		
			try 
			{
				var bytes = image.EncodeToJPG();
				File.WriteAllBytes(fullpath, bytes);
				
				RegisterTexture(fullpath, image);
			}
			catch (System.Exception e) 
			{
				Debug.LogWarning("Unable to create file! " + e.Message);
				return null;
			}

			return fullpath;
		}

		public bool RegisterFileInstance(string file, FileType type)
		{
			bool success = RegisterFile(file, type);
			return success;
		}

		bool RegisterFile(string file, FileType type, string directory = null)
		{
			bool success = true;
			bool @new = !ALL_FILES.Contains(file); // New entry to files

			bool fromUser = type == FileType.User || type == FileType.Shared;
			
			if (fromUser)  // Verify file exists (user or shared)
			{
				var fileInfo = new FileInfo(file);
				success = fileInfo.Exists;

				if (success) directory = fileInfo.Directory.FullName;
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
				
				if(fromUser) RequestTexture(file); // Request texture immediately from file
			}
		
			AddToFileLookup(type, file);

			return success;
		}

		public bool UnregisterFile(string file)
		{
			return false;
		}

		void RegisterTexture(string file, Texture tex)
		{
			var texture = (tex as Texture2D);
			bool success = (texture != null);
			
			if (TEXTURE_LOOKUP.ContainsKey(file)) TEXTURE_LOOKUP[file] = texture;
			else TEXTURE_LOOKUP.Add(file, texture);

			Debug.LogFormat("Added {0} to library", file);
			
			if (success && CREATE_THUMBNAILS) 
			{
				CreateThumbnail(file, texture);	// Generate new thumbnail from image
			}
		}

		#endregion

		#region Textures
		
		public void RequestTexture(string path, ITextureReceiver receiver = null)
		{
			if (TEXTURE_RECEIVERS.ContainsKey(path)) return;
			TEXTURE_RECEIVERS.Add(path, receiver);
			
			if (TEXTURE_LOOKUP.ContainsKey(path)) 
			{
				ReceiveTexture(path, TEXTURE_LOOKUP[path]);
				return;
			}
			
			Debug.LogWarning($"Request texture: {path}, SELF");

			TextureLoader.Push(path, this); // Push to texture loader
		}
		
		public Texture2D RequestTextureImmediate(string path)
		{
			if (TEXTURE_LOOKUP.TryGetValue(path, out Texture2D req)) 
			{
				return req;
			}

			return GetFallbackTexture(path, nullable: false);
		}

		public void ReceiveTexture(string file, Texture2D texture)
		{
			Debug.LogWarning($"Try receive file {file} texture during load? {load}");
			
			if (load) // Completing loading during intiialization
			{
				if (loadMode == LoadMode.Thumbnails) 
				{
					if (texture != null) 
					{
						var _filename = Path.GetFileNameWithoutExtension(file);
						RegisterThumbnail(_filename, texture);
					}
					
					thumbnailQueue.RemoveAt(0);
				}
				else if(loadMode == LoadMode.Files)
				{
					RegisterTexture(file, texture);
					fileQueue.RemoveAt(0);
				}

				queue.RemoveAt(0);
				textureLoadCompleted.Add(file);
				
				read = false;
			}
			else 
			{
				
				RegisterTexture(file, texture);

				Debug.LogWarning($"Try find receiver for file: {file}");
				try
				{
					ITextureReceiver receiver = TEXTURE_RECEIVERS[file]; // error
					if (receiver != null)
					{
						if (texture == null) texture = GetFallbackTexture(file, nullable: false);
						receiver.ReceiveTexture(file, texture);
					}

					TEXTURE_RECEIVERS.Remove(file); // Pop from receivers queue
				}
				catch (System.Exception e)
				{
					Debug.LogError(e);
				}

			}
		}
		

		Texture2D GetFallbackTexture(string path, bool nullable)
		{
			var _filename = Path.GetFileNameWithoutExtension(path);
			if (FALLBACK_TEXTURE_LOOKUP.ContainsKey(_filename))
				return FALLBACK_TEXTURE_LOOKUP[_filename];
			
			if (nullable) return null;
			return DEFAULT_NULL_TEXTURE;
		}

		Texture2D GetWorldTexture(string path)
		{
			foreach (KeyValuePair<string, Texture2D[]> pack in TEXTURE_PACKS) 
			{
				foreach (Texture2D tex in pack.Value) 
				{
					var textureID = tex.name + ".jpg";
					if (textureID == path) 
						return tex;
				}
			}

			return null;
		}

		#endregion
		
		#region Thumbnails
		
		string ThumbnailDirectory => Path.Combine(Path.GetFullPath(Application.persistentDataPath), "_thumbnails");

		string DefaultThumbnailPathFromFile(string filename)
		{
			var _directory = ThumbnailDirectory;
			FileUtils.EnsureDirectory(_directory, hidden:true);

			return Path.Combine(_directory, filename);
		}

		void CreateThumbnail(string path, Texture2D texture)
		{
			var _filename = Path.GetFileNameWithoutExtension(path);
			var _ext = Path.GetExtension(path);

			if (texture == null) return; // Ignore request to create thumbnail, texture is NULL!
			if (FALLBACK_TEXTURE_LOOKUP.ContainsKey(path)) return; // Ignore request to create thumbnail, already exists!

			try 
			{
				var _thumbnail = DegradeBytes((_filename + _ext), texture, (_ext == ".png" || _ext == ".PNG"));
				RegisterThumbnail(path, _thumbnail);
			}
			catch (System.Exception err) 
			{
				Debug.LogWarningFormat("Unable to create thumbnail => {0}", err.Message);
			}
		}

		void RegisterThumbnail(string file, Texture texture)
		{
			var _texture = (texture as Texture2D);
			
			if (FALLBACK_TEXTURE_LOOKUP.ContainsKey(file)) FALLBACK_TEXTURE_LOOKUP[file] = _texture;
			else FALLBACK_TEXTURE_LOOKUP.Add(file, _texture);
			
			Debug.LogFormat("Added {0} to thumbnails", file);
		}

		public byte[] ExportSheet(string filename, out int _rows, out int _columns, out Texture2D tex)
		{
			if(textureSheet != null) Destroy(textureSheet);
			
			
			var directory = Application.persistentDataPath;
			var path = Path.Combine(directory, filename + ".jpg");

			Texture2D[] thumbnails = FALLBACK_TEXTURE_LOOKUP.Values.ToArray();
			int thumbnailCount = thumbnails.Length;
			
			int rows = _rows = Mathf.Min(thumbnailCount, _MAX_DIMENSION);
			int columns = _columns = Mathf.Min( Mathf.Max(Mathf.CeilToInt((float)thumbnailCount / rows)), _MAX_DIMENSION);

			int width = columns * _WIDTH;
			int height = rows * _HEIGHT;
			
			Debug.LogWarningFormat("Export sheet!    rows: {0} columns: {1}  width: {2} height: {3}  items: {4}", rows, columns, width, height, thumbnails.Length);
			
			Texture2D sheet = new Texture2D(width, height, TextureFormat.ARGB32, false);
			
			Color[] fill = new Color[_WIDTH * _HEIGHT];
			for (int i = 0; i < fill.Length; i++) fill[i] = Color.blue; // Set default fill color

			int _x = 0, _y = 0;
			int _maxX = width, _maxY = height;
			Texture2D thumbnail = null;
			
			for (int i = 0; i < columns; i++) 
			{
				for (int j = 0; j < rows; j++) 
				{
					_x = i * _WIDTH;
					_y = j * _HEIGHT;

					var _index = j + (i * rows);
					if (_index < thumbnails.Length) thumbnail = thumbnails[_index];
					else thumbnail = null;
				
					Color[] colors = (thumbnail != null) ? thumbnail.GetPixels() : fill;
					sheet.SetPixels(_x, _y, _WIDTH, _HEIGHT, colors);
				}
			}
			sheet.Apply();

			var bytes = sheet.EncodeToPNG();
			
			File.WriteAllBytes(path, bytes);
			textureSheet = tex = sheet;
			
			return bytes;
		}
		
		#endregion
		
		#region Degradation
		
		public const int _WIDTH = 64;
		public const int _HEIGHT = 64;

		public const int _MAX_DIMENSION = 64;

		public static Texture2D DegradeTexture(Texture2D _original)
		{
			var _texture = new Texture2D(_original.width, _original.height);
			_texture.SetPixels(_original.GetPixels());
			_texture.Apply();

			TextureScale.Bilinear(_texture, _WIDTH, _HEIGHT);
			_texture.Apply();

			return _texture;
		}

		Texture2D DegradeBytes(string file, Texture2D texture, bool transparency)
		{
			string path = DefaultThumbnailPathFromFile(file);
			byte[] _data = new byte[] { };
			
			if (!File.Exists(path)) 
			{
				try 
				{
					Texture2D _texture = DegradeTexture(texture);

					if (transparency) _data = _texture.EncodeToPNG();
					else _data = _texture.EncodeToJPG();

					File.WriteAllBytes(path, _data);
					return _texture;
				}
				catch (System.Exception e) 
				{
					throw e;
				}
			}

			throw new System.Exception("File already exists, no need to create thumbnail!");
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

	public static class LibraryExtensions
	{
		public static bool IsValid(this Library lib)
		{
			Library Lib = Library.Instance;
			
			if (Lib == null) return false;
			if (lib == null) return false;

			return (lib == Lib);
		}
	}
}
