using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using uwu.IO.SimpleFileBrowser.Scripts.SimpleRecycledListView;

namespace uwu.IO.SimpleFileBrowser.Scripts
{
	public class FileBrowser : MonoBehaviour, IListViewAdapter
	{
		public enum Permission
		{
			Denied = 0,
			Granted = 1,
			ShouldAsk = 2
		}

		#region Structs

#pragma warning disable 0649
		[Serializable]
		struct FiletypeIcon
		{
			public string extension;
			public Sprite icon;
		}

		[Serializable]
		struct QuickLink
		{
#if UNITY_EDITOR || ( !UNITY_WSA && !UNITY_WSA_10_0 )
			public Environment.SpecialFolder target;
#endif
			public string name;
			public Sprite icon;
		}
#pragma warning restore 0649

		#endregion

		#region Inner Classes

		public class Filter
		{
			public readonly string defaultExtension;
			public readonly HashSet<string> extensions;
			public readonly string name;

			internal Filter(string name)
			{
				this.name = name;
				extensions = null;
				defaultExtension = null;
			}

			public Filter(string name, string extension)
			{
				this.name = name;

				extension = extension.ToLowerInvariant();
				extensions = new HashSet<string> {extension};
				defaultExtension = extension;
			}

			public Filter(string name, params string[] extensions)
			{
				this.name = name;

				for (var i = 0; i < extensions.Length; i++)
					extensions[i] = extensions[i].ToLowerInvariant();

				this.extensions = new HashSet<string>(extensions);
				defaultExtension = extensions[0];
			}

			public override string ToString()
			{
				var result = "";

				if (name != null)
					result += name;

				if (extensions != null) {
					if (name != null)
						result += " (";

					var index = 0;
					foreach (var extension in extensions)
						if (index++ > 0)
							result += ", " + extension;
						else
							result += extension;

					if (name != null)
						result += ")";
				}

				return result;
			}
		}

		#endregion

		#region Constants

		const string ALL_FILES_FILTER_TEXT = "All Files (.*)";
		const string FOLDERS_FILTER_TEXT = "Folders";
		string DEFAULT_PATH;

#if !UNITY_EDITOR && UNITY_ANDROID
		private const string SAF_PICK_FOLDER_QUICK_LINK_TEXT = "Pick Folder";
		private const string SAF_PICK_FOLDER_QUICK_LINK_PATH = "SAF_PICK_FOLDER";
#endif

		#endregion

		#region Static Variables

		public static bool IsOpen { get; private set; }

		public static bool Success { get; private set; }
		public static string Result { get; private set; }

		public static bool AskPermissions { get; set; } = true;

		public static bool SingleClickMode { get; set; } = false;

		static FileBrowser m_instance;

		static FileBrowser Instance
		{
			get
			{
				if (m_instance == null) {
					m_instance = Instantiate(Resources.Load<GameObject>("SimpleFileBrowserCanvas"))
						.GetComponent<FileBrowser>();
					DontDestroyOnLoad(m_instance.gameObject);
					m_instance.gameObject.SetActive(false);
				}

				return m_instance;
			}
		}

		#endregion

		#region Variables

#pragma warning disable 0649
		[Header("References")] [SerializeField]
		FileBrowserMovement window;

		RectTransform windowTR;

		[SerializeField] FileBrowserItem itemPrefab;

		[SerializeField] FileBrowserQuickLink quickLinkPrefab;

		[SerializeField] UnityEngine.UI.Text titleText;

		[SerializeField] Button backButton;

		[SerializeField] Button forwardButton;

		[SerializeField] Button upButton;

		[SerializeField] InputField pathInputField;

		[SerializeField] InputField searchInputField;

		[SerializeField] RectTransform quickLinksContainer;

		[SerializeField] RectTransform filesContainer;

		[SerializeField] ScrollRect filesScrollRect;

		[SerializeField] RecycledListView listView;

		[SerializeField] InputField filenameInputField;

		[SerializeField] Image filenameImage;

		[SerializeField] Dropdown filtersDropdown;

		[SerializeField] RectTransform filtersDropdownContainer;

		[SerializeField] UnityEngine.UI.Text filterItemTemplate;

		[SerializeField] Toggle showHiddenFilesToggle;

		[SerializeField] UnityEngine.UI.Text submitButtonText;

		[Header("Icons")] [SerializeField] Sprite folderIcon;

		[SerializeField] Sprite driveIcon;

		[SerializeField] Sprite defaultIcon;

		[SerializeField] FiletypeIcon[] filetypeIcons;

		Dictionary<string, Sprite> filetypeToIcon;

		[Header("Other")] public Color normalFileColor = Color.white;
		public Color hoveredFileColor = new Color32(225, 225, 255, 255);
		public Color selectedFileColor = new Color32(0, 175, 255, 255);

		public Color wrongFilenameColor = new Color32(255, 100, 100, 255);

		public int minWidth = 380;
		public int minHeight = 300;

		[SerializeField] string[] excludeExtensions;

#pragma warning disable 0414
		[SerializeField] QuickLink[] quickLinks;
#pragma warning restore 0414

		HashSet<string> excludedExtensionsSet;
		HashSet<string> addedQuickLinksSet;

		[SerializeField] bool generateQuickLinksForDrives = true;
#pragma warning restore 0649

		RectTransform rectTransform;

		FileAttributes ignoredFileAttributes = FileAttributes.System;

		FileSystemEntry[] allFileEntries;
		readonly List<FileSystemEntry> validFileEntries = new List<FileSystemEntry>();

		readonly List<Filter> filters = new List<Filter>();
		Filter allFilesFilter;

		bool showAllFilesFilter = true;

		int currentPathIndex = -1;
		readonly List<string> pathsFollowed = new List<string>();

		bool canvasDimensionsChanged;

		// Required in RefreshFiles() function
		PointerEventData nullPointerEventData;

		#endregion

		#region Properties

		string m_currentPath = string.Empty;

		string CurrentPath
		{
			get { return m_currentPath; }
			set
			{
#if !UNITY_EDITOR && UNITY_ANDROID
				if( !FileBrowserHelpers.ShouldUseSAF )
#endif
				if (value != null)
					value = GetPathWithoutTrailingDirectorySeparator(value.Trim());

				if (value == null)
					return;

				if (m_currentPath != value) {
					if (!FileBrowserHelpers.DirectoryExists(value))
						return;

					m_currentPath = value;
					pathInputField.text = m_currentPath;

					if (currentPathIndex == -1 || pathsFollowed[currentPathIndex] != m_currentPath) {
						currentPathIndex++;
						if (currentPathIndex < pathsFollowed.Count) {
							pathsFollowed[currentPathIndex] = value;
							for (var i = pathsFollowed.Count - 1; i >= currentPathIndex + 1; i--)
								pathsFollowed.RemoveAt(i);
						}
						else {
							pathsFollowed.Add(m_currentPath);
						}
					}

					backButton.interactable = currentPathIndex > 0;
					forwardButton.interactable = currentPathIndex < pathsFollowed.Count - 1;
#if !UNITY_EDITOR && UNITY_ANDROID
					if( !FileBrowserHelpers.ShouldUseSAF )
#endif
					upButton.interactable = Directory.GetParent(m_currentPath) != null;

					m_searchString = string.Empty;
					searchInputField.text = m_searchString;

					filesScrollRect.verticalNormalizedPosition = 1;

					filenameImage.color = Color.white;
					if (m_folderSelectMode)
						filenameInputField.text = string.Empty;
				}

				RefreshFiles(true);
			}
		}

		string m_searchString = string.Empty;

		string SearchString
		{
			get => m_searchString;
			set
			{
				if (m_searchString != value) {
					m_searchString = value;
					searchInputField.text = m_searchString;

					RefreshFiles(false);
				}
			}
		}

		public int SelectedFilePosition { get; set; } = -1;

		FileBrowserItem m_selectedFile;

		FileBrowserItem SelectedFile
		{
			get => m_selectedFile;
			set
			{
				if (value == null) {
					if (m_selectedFile != null)
						m_selectedFile.Deselect();

					SelectedFilePosition = -1;
					m_selectedFile = null;
				}
				else if (SelectedFilePosition != value.Position) {
					if (m_selectedFile != null)
						m_selectedFile.Deselect();

					m_selectedFile = value;
					SelectedFilePosition = value.Position;

					if (m_folderSelectMode || !m_selectedFile.IsDirectory)
						filenameInputField.text = m_selectedFile.Name;

					m_selectedFile.Select();
				}
			}
		}

		bool AcceptNonExistingFilename { get; set; }

		bool m_folderSelectMode;

		bool FolderSelectMode
		{
			get => m_folderSelectMode;
			set
			{
				if (m_folderSelectMode != value) {
					m_folderSelectMode = value;

					if (m_folderSelectMode) {
						filtersDropdown.options[0].text = FOLDERS_FILTER_TEXT;
						filtersDropdown.value = 0;
						filtersDropdown.RefreshShownValue();
						filtersDropdown.interactable = false;
					}
					else {
						filtersDropdown.options[0].text = filters[0].ToString();
						filtersDropdown.interactable = true;
					}

					var placeholder = filenameInputField.placeholder as UnityEngine.UI.Text;
					if (placeholder != null)
						placeholder.text = m_folderSelectMode ? "" : "Filename";
				}
			}
		}

		string Title
		{
			get => titleText.text;
			set => titleText.text = value;
		}

		string SubmitButtonText
		{
			get => submitButtonText.text;
			set => submitButtonText.text = value;
		}

		#endregion

		#region Delegates

		public delegate void OnSuccess(string path);

		public delegate void OnCancel();
#if !UNITY_EDITOR && UNITY_ANDROID
		public delegate void DirectoryPickCallback( string rawUri, string name );
#endif

		OnSuccess onSuccess;
		OnCancel onCancel;

		#endregion

		#region Messages

		void Awake()
		{
			m_instance = this;

			rectTransform = (RectTransform) transform;
			windowTR = (RectTransform) window.transform;

			ItemHeight = ((RectTransform) itemPrefab.transform).sizeDelta.y;
			nullPointerEventData = new PointerEventData(null);

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WSA_10_0 )
			DEFAULT_PATH = Application.persistentDataPath;
#else
			DEFAULT_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
			if( FileBrowserHelpers.ShouldUseSAF )
			{
				// These UI elements have no use in Storage Access Framework mode (Android 10+)
				upButton.gameObject.SetActive( false );
				pathInputField.gameObject.SetActive( false );
				showHiddenFilesToggle.gameObject.SetActive( false );
			}
#endif

			InitializeFiletypeIcons();
			filetypeIcons = null;

			SetExcludedExtensions(excludeExtensions);
			excludeExtensions = null;

			backButton.interactable = false;
			forwardButton.interactable = false;
			upButton.interactable = false;

			filenameInputField.onValidateInput += OnValidateFilenameInput;

			InitializeQuickLinks();
			quickLinks = null;

			allFilesFilter = new Filter(ALL_FILES_FILTER_TEXT);
			filters.Add(allFilesFilter);

			window.Initialize(this);
			listView.SetAdapter(this);
		}

		void OnRectTransformDimensionsChange()
		{
			canvasDimensionsChanged = true;
		}

		void LateUpdate()
		{
			if (canvasDimensionsChanged) {
				canvasDimensionsChanged = false;
				EnsureWindowIsWithinBounds();
			}
		}

		void OnApplicationFocus(bool focus)
		{
			if (focus)
				RefreshFiles(true);
		}

		#endregion

		#region Interface Methods

		public OnItemClickedHandler OnItemClicked
		{
			get => null;
			set { }
		}

		public int Count => validFileEntries.Count;
		public float ItemHeight { get; private set; }

		public ListItem CreateItem()
		{
			var item = Instantiate(itemPrefab, filesContainer, false);
			item.SetFileBrowser(this);

			return item;
		}

		public void SetItemContent(ListItem item)
		{
			var file = (FileBrowserItem) item;
			var fileInfo = validFileEntries[item.Position];

			var isDirectory = fileInfo.IsDirectory;

			Sprite icon;
			if (isDirectory)
				icon = folderIcon;
			else if (!filetypeToIcon.TryGetValue(fileInfo.Extension.ToLowerInvariant(), out icon))
				icon = defaultIcon;

			file.SetFile(icon, fileInfo.Name, isDirectory);
			file.SetHidden((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);

			if (item.Position == SelectedFilePosition) {
				m_selectedFile = file;
				file.Select();
			}
			else {
				file.Deselect();
			}
		}

		#endregion

		#region Initialization Functions

		void InitializeFiletypeIcons()
		{
			filetypeToIcon = new Dictionary<string, Sprite>();
			for (var i = 0; i < filetypeIcons.Length; i++) {
				var thisIcon = filetypeIcons[i];
				filetypeToIcon[thisIcon.extension] = thisIcon.icon;
			}
		}

		void InitializeQuickLinks()
		{
			addedQuickLinksSet = new HashSet<string>();

			var anchoredPos = new Vector2(0f, -quickLinksContainer.sizeDelta.y);

#if !UNITY_EDITOR && UNITY_ANDROID
			if( !FileBrowserHelpers.ShouldUseSAF )
			{
#endif
			if (generateQuickLinksForDrives) {
#if !UNITY_EDITOR && UNITY_ANDROID
				string drivesList = FileBrowserHelpers.AJC.CallStatic<string>( "GetExternalDrives" );
				if( drivesList != null && drivesList.Length > 0 )
				{
					bool defaultPathInitialized = false;
					int driveIndex = 1;
					string[] drives = drivesList.Split( ':' );
					for( int i = 0; i < drives.Length; i++ )
					{
						try
						{
							//string driveName = new DirectoryInfo( drives[i] ).Name;
							//if( driveName.Length <= 1 )
							//{
							//	try
							//	{
							//		driveName = Directory.GetParent( drives[i] ).Name + "/" + driveName;
							//	}
							//	catch
							//	{
							//		driveName = "Drive " + driveIndex++;
							//	}
							//}	

							string driveName;
							if( !defaultPathInitialized )
							{
								DEFAULT_PATH = drives[i];
								defaultPathInitialized = true;

								driveName = "Primary Drive";
							}
							else
							{
								if( driveIndex == 1 )
									driveName = "External Drive";
								else
									driveName = "External Drive " + driveIndex;

								driveIndex++;
							}

							AddQuickLink( driveIcon, driveName, drives[i], ref anchoredPos );
						}
						catch { }
					}
				}
#elif !UNITY_EDITOR && ( UNITY_IOS || UNITY_WSA || UNITY_WSA_10_0 )
				AddQuickLink( driveIcon, "Files", Application.persistentDataPath, ref anchoredPos );
#else
				var drives = Directory.GetLogicalDrives();

				for (var i = 0; i < drives.Length; i++)
					AddQuickLink(driveIcon, drives[i], drives[i], ref anchoredPos);
#endif
			}

#if UNITY_EDITOR || ( !UNITY_ANDROID && !UNITY_WSA && !UNITY_WSA_10_0 )
			for (var i = 0; i < quickLinks.Length; i++) {
				var quickLink = quickLinks[i];
				var quickLinkPath = Environment.GetFolderPath(quickLink.target);

				AddQuickLink(quickLink.icon, quickLink.name, quickLinkPath, ref anchoredPos);
			}
#endif
#if !UNITY_EDITOR && UNITY_ANDROID
			}
			else
			{
				AddQuickLink( driveIcon, SAF_PICK_FOLDER_QUICK_LINK_TEXT, SAF_PICK_FOLDER_QUICK_LINK_PATH, ref anchoredPos );
				
				try
				{
					FetchPersistedSAFQuickLinks( ref anchoredPos );
				}
				catch( Exception e )
				{
					Debug.LogException( e );
				}
			}
#endif

			quickLinksContainer.sizeDelta = new Vector2(0f, -anchoredPos.y);
		}

		#endregion

		#region Button Events

		public void OnBackButtonPressed()
		{
			if (currentPathIndex > 0)
				CurrentPath = pathsFollowed[--currentPathIndex];
		}

		public void OnForwardButtonPressed()
		{
			if (currentPathIndex < pathsFollowed.Count - 1)
				CurrentPath = pathsFollowed[++currentPathIndex];
		}

		public void OnUpButtonPressed()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			if( FileBrowserHelpers.ShouldUseSAF )
				return;
#endif
			var parentPath = Directory.GetParent(m_currentPath);

			if (parentPath != null)
				CurrentPath = parentPath.FullName;
		}

		public void OnSubmitButtonClicked()
		{
			var path = m_currentPath;
			var filenameInput = filenameInputField.text.Trim();

#if !UNITY_EDITOR && UNITY_ANDROID
			if( FileBrowserHelpers.ShouldUseSAF )
			{
				if( filenameInput.Length == 0 )
				{
					if( m_folderSelectMode )
						OnOperationSuccessful( path );
					else
						filenameImage.color = wrongFilenameColor;

					return;
				}

				for( int i = 0; i < validFileEntries.Count; i++ )
				{
					FileSystemEntry fileInfo = validFileEntries[i];
					if( fileInfo.Name == filenameInput )
					{
						if( fileInfo.IsDirectory == m_folderSelectMode )
							OnOperationSuccessful( fileInfo.Path );
						else if( fileInfo.IsDirectory )
							CurrentPath = fileInfo.Path;
						else
							filenameImage.color = wrongFilenameColor;

						return;
					}
				}

				if( m_acceptNonExistingFilename )
				{
					if( !m_folderSelectMode && filters[filtersDropdown.value].defaultExtension != null )
						filenameInput =
 Path.ChangeExtension( filenameInput, filters[filtersDropdown.value].defaultExtension );

					if( m_folderSelectMode )
						OnOperationSuccessful( FileBrowserHelpers.CreateFolderInDirectory( path, filenameInput ) );
					else
						OnOperationSuccessful( FileBrowserHelpers.CreateFileInDirectory( path, filenameInput ) );
				}
				else
					filenameImage.color = wrongFilenameColor;

				return;
			}
#endif

			if (filenameInput.Length > 0)
				path = Path.Combine(path, filenameInput);

			if (File.Exists(path)) {
				if (!m_folderSelectMode)
					OnOperationSuccessful(path);
				else
					filenameImage.color = wrongFilenameColor;
			}
			else if (Directory.Exists(path)) {
				if (m_folderSelectMode) {
					OnOperationSuccessful(path);
				}
				else {
					if (m_currentPath == path)
						filenameImage.color = wrongFilenameColor;
					else
						CurrentPath = path;
				}
			}
			else {
				if (AcceptNonExistingFilename) {
					if (!m_folderSelectMode && filters[filtersDropdown.value].defaultExtension != null)
						path = Path.ChangeExtension(path, filters[filtersDropdown.value].defaultExtension);

					OnOperationSuccessful(path);
				}
				else {
					filenameImage.color = wrongFilenameColor;
				}
			}
		}

		public void OnCancelButtonClicked()
		{
			OnOperationCanceled(true);
		}

		#endregion

		#region Other Events

		void OnOperationSuccessful(string path)
		{
			Success = true;
			Result = path;

			Hide();

			var _onSuccess = onSuccess;
			onSuccess = null;
			onCancel = null;

			if (_onSuccess != null)
				_onSuccess(path);
		}

		void OnOperationCanceled(bool invokeCancelCallback)
		{
			Success = false;
			Result = null;

			Hide();

			var _onCancel = onCancel;
			onSuccess = null;
			onCancel = null;

			if (invokeCancelCallback && _onCancel != null)
				_onCancel();
		}

		public void OnPathChanged(string newPath)
		{
			CurrentPath = newPath;
		}

		public void OnSearchStringChanged(string newSearchString)
		{
			SearchString = newSearchString;
		}

		public void OnFilterChanged()
		{
			RefreshFiles(false);
		}

		public void OnShowHiddenFilesToggleChanged()
		{
			RefreshFiles(false);
		}

		public void OnQuickLinkSelected(FileBrowserQuickLink quickLink)
		{
			if (quickLink != null) {
#if !UNITY_EDITOR && UNITY_ANDROID
				if( quickLink.TargetPath == SAF_PICK_FOLDER_QUICK_LINK_PATH )
					FileBrowserHelpers.AJC.CallStatic( "PickSAFFolder", FileBrowserHelpers.Context, new FBDirectoryReceiveCallbackAndroid( OnSAFDirectoryPicked ) );
				else
#endif
				CurrentPath = quickLink.TargetPath;
			}
		}

		public void OnItemSelected(FileBrowserItem item)
		{
			SelectedFile = item;
		}

		public void OnItemOpened(FileBrowserItem item)
		{
			if (item.IsDirectory) {
#if !UNITY_EDITOR && UNITY_ANDROID
				if( FileBrowserHelpers.ShouldUseSAF )
				{
					for( int i = 0; i < validFileEntries.Count; i++ )
					{
						FileSystemEntry fileInfo = validFileEntries[i];
						if( fileInfo.IsDirectory && fileInfo.Name == item.Name )
						{
							CurrentPath = fileInfo.Path;
							return;
						}
					}
				}
				else
#endif
				CurrentPath = Path.Combine(m_currentPath, item.Name);
			}
			else {
				OnSubmitButtonClicked();
			}
		}

#if !UNITY_EDITOR && UNITY_ANDROID
		private void OnSAFDirectoryPicked( string rawUri, string name )
		{
			if( !string.IsNullOrEmpty( rawUri ) )
			{
				Vector2 anchoredPos = new Vector2( 0f, -quickLinksContainer.sizeDelta.y );

				if( AddQuickLink( folderIcon, name, rawUri, ref anchoredPos ) )
				{
					quickLinksContainer.sizeDelta = new Vector2( 0f, -anchoredPos.y );
					CurrentPath = rawUri;
				}
			}
		}

		private void FetchPersistedSAFQuickLinks( ref Vector2 anchoredPos )
		{
			string resultRaw =
 FileBrowserHelpers.AJC.CallStatic<string>( "FetchSAFQuickLinks", FileBrowserHelpers.Context );
			if( resultRaw == "0" )
				return;

			int separatorIndex = resultRaw.LastIndexOf( "<>" );
			if( separatorIndex <= 0 )
			{
				Debug.LogError( "Entry count does not exist" );
				return;
			}

			int entryCount = 0;
			for( int i = separatorIndex + 2; i < resultRaw.Length; i++ )
			{
				char ch = resultRaw[i];
				if( ch < '0' && ch > '9' )
				{
					Debug.LogError( "Couldn't parse entry count" );
					return;
				}
					
				entryCount = entryCount * 10 + ( ch - '0' );
			}

			if( entryCount <= 0 )
				return;
		
			bool defaultPathInitialized = false;

			separatorIndex = 0;
			for( int i = 0; i < entryCount; i++ )
			{
				int nextSeparatorIndex = resultRaw.IndexOf( "<>", separatorIndex );
				if( nextSeparatorIndex <= 0 )
				{
					Debug.LogError( "Entry name is empty" );
					return;
				}

				string entryName = resultRaw.Substring( separatorIndex, nextSeparatorIndex - separatorIndex );

				separatorIndex = nextSeparatorIndex + 2;
				nextSeparatorIndex = resultRaw.IndexOf( "<>", separatorIndex );
				if( nextSeparatorIndex <= 0 )
				{
					Debug.LogError( "Entry rawUri is empty" );
					return;
				}

				string rawUri = resultRaw.Substring( separatorIndex, nextSeparatorIndex - separatorIndex );

				separatorIndex = nextSeparatorIndex + 2;

				if( AddQuickLink( folderIcon, entryName, rawUri, ref anchoredPos ) && !defaultPathInitialized )
				{
					DEFAULT_PATH = rawUri;
					defaultPathInitialized = true;
				}
			}
		}
#endif

		public char OnValidateFilenameInput(string text, int charIndex, char addedChar)
		{
			if (addedChar == '\n') {
				OnSubmitButtonClicked();
				return '\0';
			}

			return addedChar;
		}

		#endregion

		#region Helper Functions

		public void Show(string initialPath)
		{
			if (AskPermissions)
				RequestPermission();

			SelectedFile = null;

			m_searchString = string.Empty;
			searchInputField.text = m_searchString;

			filesScrollRect.verticalNormalizedPosition = 1;

			filenameInputField.text = string.Empty;
			filenameImage.color = Color.white;

			IsOpen = true;
			Success = false;
			Result = null;

			gameObject.SetActive(true);

			CurrentPath = GetInitialPath(initialPath);
		}

		public void Hide()
		{
			IsOpen = false;

			currentPathIndex = -1;
			pathsFollowed.Clear();

			backButton.interactable = false;
			forwardButton.interactable = false;
			upButton.interactable = false;

			gameObject.SetActive(false);
		}

		public void RefreshFiles(bool pathChanged)
		{
			if (pathChanged) {
				if (!string.IsNullOrEmpty(m_currentPath))
					allFileEntries = FileBrowserHelpers.GetEntriesInDirectory(m_currentPath);
				else
					allFileEntries = null;
			}

			SelectedFile = null;

			if (!showHiddenFilesToggle.isOn)
				ignoredFileAttributes |= FileAttributes.Hidden;
			else
				ignoredFileAttributes &= ~FileAttributes.Hidden;

			var searchStringLowercase = m_searchString.ToLower();

			validFileEntries.Clear();

			if (allFileEntries != null)
				for (var i = 0; i < allFileEntries.Length; i++)
					try {
						var item = allFileEntries[i];

						if (!item.IsDirectory) {
							if (m_folderSelectMode)
								continue;

							if ((item.Attributes & ignoredFileAttributes) != 0)
								continue;

							var extension = item.Extension.ToLowerInvariant();
							if (excludedExtensionsSet.Contains(extension))
								continue;

							var extensions = filters[filtersDropdown.value].extensions;
							if (extensions != null && !extensions.Contains(extension))
								continue;
						}
						else {
							if ((item.Attributes & ignoredFileAttributes) != 0)
								continue;
						}

						if (m_searchString.Length == 0 || item.Name.ToLower().Contains(searchStringLowercase))
							validFileEntries.Add(item);
					}
					catch (Exception e) {
						Debug.LogException(e);
					}

			listView.UpdateList();

			// Prevent the case where all the content stays offscreen after changing the search string
			filesScrollRect.OnScroll(nullPointerEventData);
		}

		bool AddQuickLink(Sprite icon, string name, string path, ref Vector2 anchoredPos)
		{
			if (string.IsNullOrEmpty(path))
				return false;

#if !UNITY_EDITOR && UNITY_ANDROID
			if( !FileBrowserHelpers.ShouldUseSAF )
#endif
			if (!Directory.Exists(path))
				return false;

			// Don't add quick link if it already exists
			if (addedQuickLinksSet.Contains(path))
				return false;

			var quickLink = Instantiate(quickLinkPrefab, quickLinksContainer, false);
			quickLink.SetFileBrowser(this);

			if (icon != null)
				quickLink.SetQuickLink(icon, name, path);
			else
				quickLink.SetQuickLink(folderIcon, name, path);

			quickLink.TransformComponent.anchoredPosition = anchoredPos;
			anchoredPos.y -= ItemHeight;

			addedQuickLinksSet.Add(path);

			return true;
		}

		public void EnsureWindowIsWithinBounds()
		{
			var canvasSize = rectTransform.sizeDelta;
			var windowSize = windowTR.sizeDelta;

			if (windowSize.x > canvasSize.x)
				windowSize.x = canvasSize.x;
			if (windowSize.y > canvasSize.y)
				windowSize.y = canvasSize.y;

			var windowPos = windowTR.anchoredPosition;
			var canvasHalfSize = canvasSize * 0.5f;
			var windowHalfSize = windowSize * 0.5f;
			var windowBottomLeft = windowPos - windowHalfSize + canvasHalfSize;
			var windowTopRight = windowPos + windowHalfSize + canvasHalfSize;

			if (windowBottomLeft.x < 0f)
				windowPos.x -= windowBottomLeft.x;
			else if (windowTopRight.x > canvasSize.x)
				windowPos.x -= windowTopRight.x - canvasSize.x;

			if (windowBottomLeft.y < 0f)
				windowPos.y -= windowBottomLeft.y;
			else if (windowTopRight.y > canvasSize.y)
				windowPos.y -= windowTopRight.y - canvasSize.y;

			windowTR.anchoredPosition = windowPos;
			windowTR.sizeDelta = windowSize;
		}

		string GetPathWithoutTrailingDirectorySeparator(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;

			// Credit: http://stackoverflow.com/questions/6019227/remove-the-last-character-if-its-directoryseparatorchar-with-c-sharp
			try {
				if (Path.GetDirectoryName(path) != null) {
					var lastChar = path[path.Length - 1];
					if (lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar)
						path = path.Substring(0, path.Length - 1);
				}
			}
			catch {
				return null;
			}

			return path;
		}

		// Credit: http://answers.unity3d.com/questions/898770/how-to-get-the-width-of-ui-text-with-horizontal-ov.html
		int CalculateLengthOfDropdownText(string str)
		{
			var totalLength = 0;

			var myFont = filterItemTemplate.font;
			var characterInfo = new CharacterInfo();

			myFont.RequestCharactersInTexture(str, filterItemTemplate.fontSize, filterItemTemplate.fontStyle);

			for (var i = 0; i < str.Length; i++) {
				if (!myFont.GetCharacterInfo(str[i], out characterInfo, filterItemTemplate.fontSize))
					totalLength += 5;

				totalLength += characterInfo.advance;
			}

			return totalLength;
		}

		string GetInitialPath(string initialPath)
		{
			if (string.IsNullOrEmpty(initialPath) || !Directory.Exists(initialPath)) {
				if (CurrentPath.Length == 0)
					initialPath = DEFAULT_PATH;
				else
					initialPath = CurrentPath;
			}

			m_currentPath = string.Empty; // Needed to correctly reset the pathsFollowed

			return initialPath;
		}

		#endregion

		#region File Browser Functions (static)

		public static bool ShowSaveDialog(OnSuccess onSuccess, OnCancel onCancel,
			bool folderMode = false, string initialPath = null,
			string title = "Save", string saveButtonText = "Save")
		{
			if (Instance.gameObject.activeSelf) {
				Debug.LogError("Error: Multiple dialogs are not allowed!");
				return false;
			}

			Instance.onSuccess = onSuccess;
			Instance.onCancel = onCancel;

			Instance.FolderSelectMode = folderMode;
			Instance.Title = title;
			Instance.SubmitButtonText = saveButtonText;
			Instance.AcceptNonExistingFilename = !folderMode;

			Instance.Show(initialPath);

			return true;
		}

		public static bool ShowLoadDialog(OnSuccess onSuccess, OnCancel onCancel,
			bool folderMode = false, string initialPath = null,
			string title = "Load", string loadButtonText = "Select")
		{
			if (Instance.gameObject.activeSelf) {
				Debug.LogError("Error: Multiple dialogs are not allowed!");
				return false;
			}

			Instance.onSuccess = onSuccess;
			Instance.onCancel = onCancel;

			Instance.FolderSelectMode = folderMode;
			Instance.Title = title;
			Instance.SubmitButtonText = loadButtonText;
			Instance.AcceptNonExistingFilename = false;

			Instance.Show(initialPath);

			return true;
		}

		public static void HideDialog(bool invokeCancelCallback = false)
		{
			Instance.OnOperationCanceled(invokeCancelCallback);
		}

		public static IEnumerator WaitForSaveDialog(bool folderMode = false, string initialPath = null,
			string title = "Save", string saveButtonText = "Save")
		{
			if (!ShowSaveDialog(null, null, folderMode, initialPath, title, saveButtonText))
				yield break;

			while (Instance.gameObject.activeSelf)
				yield return null;
		}

		public static IEnumerator WaitForLoadDialog(bool folderMode = false, string initialPath = null,
			string title = "Load", string loadButtonText = "Select")
		{
			if (!ShowLoadDialog(null, null, folderMode, initialPath, title, loadButtonText))
				yield break;

			while (Instance.gameObject.activeSelf)
				yield return null;
		}

		public static bool AddQuickLink(string name, string path, Sprite icon = null)
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			if( FileBrowserHelpers.ShouldUseSAF )
				return false;
#endif

			var anchoredPos = new Vector2(0f, -Instance.quickLinksContainer.sizeDelta.y);

			if (Instance.AddQuickLink(icon, name, path, ref anchoredPos)) {
				Instance.quickLinksContainer.sizeDelta = new Vector2(0f, -anchoredPos.y);
				return true;
			}

			return false;
		}

		public static void SetExcludedExtensions(params string[] excludedExtensions)
		{
			if (Instance.excludedExtensionsSet == null)
				Instance.excludedExtensionsSet = new HashSet<string>();
			else
				Instance.excludedExtensionsSet.Clear();

			if (excludedExtensions != null)
				for (var i = 0; i < excludedExtensions.Length; i++)
					Instance.excludedExtensionsSet.Add(excludedExtensions[i].ToLowerInvariant());
		}

		public static void SetFilters(bool showAllFilesFilter, IEnumerable<string> filters)
		{
			SetFiltersPreProcessing(showAllFilesFilter);

			if (filters != null)
				foreach (var filter in filters)
					if (filter != null && filter.Length > 0)
						Instance.filters.Add(new Filter(null, filter));

			SetFiltersPostProcessing();
		}

		public static void SetFilters(bool showAllFilesFilter, params string[] filters)
		{
			SetFiltersPreProcessing(showAllFilesFilter);

			if (filters != null)
				for (var i = 0; i < filters.Length; i++)
					if (filters[i] != null && filters[i].Length > 0)
						Instance.filters.Add(new Filter(null, filters[i]));

			SetFiltersPostProcessing();
		}

		public static void SetFilters(bool showAllFilesFilter, IEnumerable<Filter> filters)
		{
			SetFiltersPreProcessing(showAllFilesFilter);

			if (filters != null)
				foreach (var filter in filters)
					if (filter != null && filter.defaultExtension.Length > 0)
						Instance.filters.Add(filter);

			SetFiltersPostProcessing();
		}

		public static void SetFilters(bool showAllFilesFilter, params Filter[] filters)
		{
			SetFiltersPreProcessing(showAllFilesFilter);

			if (filters != null)
				for (var i = 0; i < filters.Length; i++)
					if (filters[i] != null && filters[i].defaultExtension.Length > 0)
						Instance.filters.Add(filters[i]);

			SetFiltersPostProcessing();
		}

		static void SetFiltersPreProcessing(bool showAllFilesFilter)
		{
			Instance.showAllFilesFilter = showAllFilesFilter;

			Instance.filters.Clear();

			if (showAllFilesFilter)
				Instance.filters.Add(Instance.allFilesFilter);
		}

		static void SetFiltersPostProcessing()
		{
			var filters = Instance.filters;

			if (filters.Count == 0)
				filters.Add(Instance.allFilesFilter);

			var maxFilterStrLength = 100;
			var dropdownValues = new List<string>(filters.Count);
			for (var i = 0; i < filters.Count; i++) {
				var filterStr = filters[i].ToString();
				dropdownValues.Add(filterStr);

				maxFilterStrLength = Mathf.Max(maxFilterStrLength, Instance.CalculateLengthOfDropdownText(filterStr));
			}

			var size = Instance.filtersDropdownContainer.sizeDelta;
			size.x = maxFilterStrLength + 28;
			Instance.filtersDropdownContainer.sizeDelta = size;

			Instance.filtersDropdown.ClearOptions();
			Instance.filtersDropdown.AddOptions(dropdownValues);
			Instance.filtersDropdown.value = 0;
		}

		public static bool SetDefaultFilter(string defaultFilter)
		{
			if (defaultFilter == null) {
				if (Instance.showAllFilesFilter) {
					Instance.filtersDropdown.value = 0;
					Instance.filtersDropdown.RefreshShownValue();

					return true;
				}

				return false;
			}

			defaultFilter = defaultFilter.ToLowerInvariant();

			for (var i = 0; i < Instance.filters.Count; i++) {
				var extensions = Instance.filters[i].extensions;
				if (extensions != null && extensions.Contains(defaultFilter)) {
					Instance.filtersDropdown.value = i;
					Instance.filtersDropdown.RefreshShownValue();

					return true;
				}
			}

			return false;
		}

		public static Permission CheckPermission()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			Permission result =
 (Permission) FileBrowserHelpers.AJC.CallStatic<int>( "CheckPermission", FileBrowserHelpers.Context );
			if( result == Permission.Denied && (Permission) PlayerPrefs.GetInt( "FileBrowserPermission", (int) Permission.ShouldAsk ) == Permission.ShouldAsk )
				result = Permission.ShouldAsk;

			return result;
#else
			return Permission.Granted;
#endif
		}

		public static Permission RequestPermission()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			object threadLock = new object();
			lock( threadLock )
			{
				FBPermissionCallbackAndroid nativeCallback = new FBPermissionCallbackAndroid( threadLock );

				FileBrowserHelpers.AJC.CallStatic( "RequestPermission", FileBrowserHelpers.Context, nativeCallback, PlayerPrefs.GetInt( "FileBrowserPermission", (int) Permission.ShouldAsk ) );

				if( nativeCallback.Result == -1 )
					System.Threading.Monitor.Wait( threadLock );

				if( (Permission) nativeCallback.Result != Permission.ShouldAsk && PlayerPrefs.GetInt( "FileBrowserPermission", -1 ) != nativeCallback.Result )
				{
					PlayerPrefs.SetInt( "FileBrowserPermission", nativeCallback.Result );
					PlayerPrefs.Save();
				}

				return (Permission) nativeCallback.Result;
			}
#else
			return Permission.Granted;
#endif
		}

		#endregion
	}
}