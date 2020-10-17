using UnityEngine;
using UnityEngine.EventSystems;

namespace uwu.IO.SimpleFileBrowser.Scripts
{
	public class FileBrowserQuickLink : FileBrowserItem, IPointerClickHandler
	{
		#region Pointer Events

		public new void OnPointerClick(PointerEventData eventData)
		{
			fileBrowser.OnQuickLinkSelected(this);
		}

		#endregion

		#region Initialization Functions

		public void SetQuickLink(Sprite icon, string name, string targetPath)
		{
			SetFile(icon, name, true);

			TargetPath = targetPath;
		}

		#endregion

		#region Properties

		public string TargetPath { get; set; }

		#endregion

		#region Other Events

		#endregion
	}
}