using System;
using butterflowersOS.Core;
using TMPro;
using UnityEngine;

namespace Objects.Managers
{
	[Obsolete("Obsolete API!", true)]
	public class LoadingManager : MonoBehaviour
	{
		// External
		
		Library Lib;
		
		// Properties

		SummaryManager Summary;
		[SerializeField] TMP_Text loadingPrompt = null;
		
		// Attributes

		[SerializeField] string defaultText = "";
		[SerializeField] string foundFilesText = "";
		[SerializeField] string noFilesText = "";
		
		#region Monobehaviour callbacks

		void Start()
		{
			Summary = GetComponentInParent<SummaryManager>();
			
			Lib = Library.Instance;
			Lib.onAddedFiles += onAddedFilesToDesktop;
		}

		void OnDestroy()
		{
			if(Lib.IsValid()) Lib.onAddedFiles -= onAddedFilesToDesktop;
		}

		#endregion
		
		#region Show and hide

		public void ShowLoading()
		{
			loadingPrompt.text = defaultText;
		}
		
		#endregion
		
		#region File discovery

		void onAddedFilesToDesktop(string[] files)
		{
			if (!Summary.active || Summary.ActivePanel != SummaryManager.Panel.Reload) return;

			if (files.Length > 0)
				loadingPrompt.text = string.Format(foundFilesText, files.Length);
			else
				loadingPrompt.text = noFilesText;
		}
		
		#endregion
	}
}