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
		TextureLoader TextureLoader = null;

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

		// Attributes

		[SerializeField] bool read = false, load = false, initialized = false;
		[SerializeField] bool listenForEvents = false;

		[SerializeField] LoadMode loadMode;

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

		void Start()
		{
			TextureLoader = TextureLoader.Instance;
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

		public void Load(LibraryPayload payload, Texture2D NULL_TEXTURE, Dictionary<string, Texture2D[]> TEXTURE_PACKS, bool loadTexturesImmediate, bool createThumbnails)
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
			
			LoadThumbnails();
			GenerateThumbnails();
			LoadFiles();

			textureLoadTarget = generateThumbnailQueue.Union(thumbnailQueue).Union(fileQueue).ToList();
			if (textureLoadTarget.Count > 0) 
			{
				if (generateThumbnailQueue.Count > 0) loadMode = LoadMode.Generate;
				else if (thumbnailQueue.Count > 0) loadMode = LoadMode.Thumbnails;
				else if (fileQueue.Count > 0) loadMode = LoadMode.Files;
				
				StartCoroutine("LoadingAllFiles");
			}
			else 
			{
				loadMode = LoadMode.NULL;
			}
			
			
			initialized = true;
			load = (loadMode != LoadMode.NULL);
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
			queue.AddRange(fileQueue);

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
						if (thumbnailQueue.Count <= 0) loadMode = LoadMode.Files;
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
			
				RegisterFile(filename, type);
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

			if (type == FileType.User || type == FileType.Shared)  // Verify file exists (user or shared)
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

				ITextureReceiver receiver = TEXTURE_RECEIVERS[file];
				if (receiver != null) 
				{
					if (texture == null) texture = GetFallbackTexture(file, nullable: false);
					receiver.ReceiveTexture(file, texture);
				}

				TEXTURE_RECEIVERS.Remove(file); // Pop from receivers queue
				
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
			FileUtils.EnsureDirectory(_directory);

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
		
		#endregion
		
		#region Degradation
		
		const int _WIDTH = 32;
		const int _HEIGHT = 32;

		Texture2D DegradeBytes(string file, Texture2D texture, bool transparency)
		{
			var width = texture.width;
			var height = texture.height;

			string path = DefaultThumbnailPathFromFile(file);
			byte[] _data = new byte[] { };
			
			if (!File.Exists(path)) 
			{
				try 
				{
					var _texture = new Texture2D(texture.width, texture.height);
					_texture.SetPixels(texture.GetPixels());
					_texture.Apply();

					bool resize = (width > _WIDTH || height > _HEIGHT);
					if (resize) {
						TextureScale.Bilinear(_texture, _WIDTH, _HEIGHT);
						_texture.Apply();
					}

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
}
