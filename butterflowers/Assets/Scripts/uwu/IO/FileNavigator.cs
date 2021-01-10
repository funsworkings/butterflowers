using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using uwu.IO.SimpleFileBrowser.Scripts;
using Enviro = System.Environment;
using Files = uwu.IO.SimpleFileBrowser.Scripts.FileBrowserHelpers;


namespace uwu.IO
{
	public class FileNavigator : MonoBehaviour
	{
		#region Internal

		public enum FileType
		{
			Normal,
			Directory,
			All
		}

		#endregion

		public static FileNavigator Instance;

		#region Events

		public Action onRefresh;

		#endregion

		#region Instance helpers

		public bool IsFileVisible(string path = null)
		{
			if (string.IsNullOrEmpty(path)) return false;
			return fileLookup.ContainsKey(path);
		}

		#endregion

		#region Attributes

		[SerializeField] Environment.SpecialFolder root;
		[SerializeField] string[] filters = { };

		#endregion

		#region Collections

		[SerializeField] string path = "";
		[SerializeField] FileSystemEntry folder;

		Dictionary<string, FileSystemEntry> fileLookup = new Dictionary<string, FileSystemEntry>();
		Dictionary<string, FileSystemEntry> directoryLookup = new Dictionary<string, FileSystemEntry>();

		[SerializeField] FileSystemEntry[] files = { };
		[SerializeField] FileSystemEntry[] directories = { };

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			Instance = this;
		}

		void OnEnable()
		{
			if (string.IsNullOrEmpty(path))
				path = Enviro.GetFolderPath(root); // Set to root folder if path is null
		}

		void OnApplicationPause(bool paused)
		{
			return;

			if (!paused)
				Refresh(); // Refresh all files in directory
		}

		void OnApplicationFocus(bool focused)
		{
			return;

			if (focused)
				Refresh();
		}

		#endregion

		#region Helpers

		public static bool isPathDirectory(string path)
		{
			if (!string.IsNullOrEmpty(path))
				try {
					var attr = File.GetAttributes(path);
					return attr.HasFlag(FileAttributes.Directory);
				}
				catch (Exception e) {
					Debug.LogException(e);
				}

			return false;
		}

		public static FileSystemEntry GetParent(string path)
		{
			FileSystemEntry parent = null;

			if (isPathDirectory(path)) {
				var par = Directory.GetParent(path);
				if (par != null) {
					var name = System.IO.Path.GetDirectoryName(path);
					var entry = new FileSystemEntry(par.FullName, name, true);

					parent = entry;
				}
			}

			return parent;
		}

		public static FileSystemEntry GetFileInDirectory(string directory = null, int index = 0,
			FileType type = FileType.All)
		{
			FileSystemEntry file = null;

			var isDirectory = isPathDirectory(directory);
			if (isDirectory) {
				var files = GetFilesInDirectory(directory, type);

				if (files.Length > 0) {
					if (index < 0)
						index = 0;
					else if (index > files.Length - 1)
						index = files.Length - 1;

					file = files[index];
				}
			}

			return file;
		}

		public bool ExtensionMatchesFilter(string ext)
		{
			if (filters == null || filters.Length == 0) {
				return true;
			}
			else {
				return (filters.Contains(ext));
			}
		}

		public static FileSystemEntry[] GetFilesInDirectory(string path, FileType type = FileType.All,
			string[] filters = null)
		{
			var entries = new List<FileSystemEntry>();

			var allTypes = type == FileType.All;

			var files = Files.GetEntriesInDirectory(path);
			if (files != null)
				foreach (var file in files)
					try {
						var directory = file.IsDirectory;
						if (!directory) // Normal file
						{
							if (allTypes || type == FileType.Normal) {
								if (filters == null || filters.Length == 0) {
									entries.Add(file); // Automatically add filters to returned 
								}
								else {
									var extension = file.Extension.ToLowerInvariant();
									if (filters.Contains(extension))
										entries.Add(file);
								}
							}
						}
						else {
							if (allTypes || type == FileType.Directory) // Directory
								entries.Add(file);
						}
					}
					catch (Exception e) {
						Debug.LogException(e);
					}

			return entries.ToArray();
		}

		public string[] GetPathsFromFiles(FileSystemEntry[] files)
		{
			var temp = new List<string>();
			for (var i = 0; i < files.Length; i++)
				temp.Add(files[i].Path);

			return temp.ToArray();
		}

		public FileSystemEntry[] GetFilesFromPaths(string[] paths)
		{
			var valid = paths.Where(path => fileLookup.ContainsKey(path));
			if (valid.Count() == 0) return null;

			var files = new List<FileSystemEntry>();
			foreach (var path in valid)
				files.Add(fileLookup[path]);

			return files.ToArray();
		}

		#endregion

		#region Operations

		public void Refresh()
		{
			files = GetFilesInDirectory(path, FileType.Normal, filters);
			directories = GetFilesInDirectory(path, FileType.Directory);

			fileLookup = new Dictionary<string, FileSystemEntry>();
			for (var i = 0; i < files.Length; i++) {
				var file = files[i];

				try {
					var path = file.Path;
					fileLookup.Add(path, file);
				}
				catch (Exception e) {
					Debug.LogWarning(e);
				}
			}

			directoryLookup = new Dictionary<string, FileSystemEntry>();
			for (var i = 0; i < directories.Length; i++) {
				var directory = directories[i];

				try {
					directoryLookup.Add(directory.Name, directory);
				}
				catch (Exception e) {
					Debug.LogWarning(e);
				}
			}

			Debug.LogFormat("Refreshed navigator~  FILES:{0} | DIRECTORIES:{1}", files.Length, directories.Length);
			if (onRefresh != null)
				onRefresh();
		}

		public bool SetPath(string path)
		{
			var refresh = this.path != path;
			if (!Directory.Exists(path))
				return false; // Ignore path override when directory is invalid

			this.path = path;
			folder = new FileSystemEntry(path, path, true);
			if (refresh)
				Refresh();

			return true;
		}

		public void Ascend()
		{
			var directory = GetParent(path);
			if (directory != null) SetPath(directory.Path);
		}

		public void Descend(string directory)
		{
			if (string.IsNullOrEmpty(directory)) return;
			if (!directoryLookup.ContainsKey(directory)) return;

			SetPath(directory);
		}

		#endregion

		#region Accessors

		public string Path => path;

		public FileSystemEntry[] GetFiles()
		{
			return files;
		}

		public FileSystemEntry[] GetFiles(string directory)
		{
			if(!Directory.Exists(directory)) return new FileSystemEntry[]{};
			return GetFilesInDirectory(directory, FileType.Normal, filters);
		}

		public string[] GetPaths()
		{
			return fileLookup.Keys.ToArray();
		}

		public FileSystemEntry[] GetDirectories()
		{
			return directories;
		}

		public FileSystemEntry GetParent()
		{
			return GetParent(path);
		}

		#endregion
	}
}