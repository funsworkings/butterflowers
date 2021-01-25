using System.Collections;
using butterflowersOS.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uwu;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Objects.Managers
{
	public class TutorialManager : MonoBehaviour
	{
		// Collections
		
		[SerializeField] TutorialPanel[] panels = new TutorialPanel[]{};
		
		// Properties

		public bool inprogress = false, dispose = false;
		public int index = -1;
		
		[SerializeField] GameObject root;
		[SerializeField] ToggleOpacity opacity;
		[SerializeField] TMP_Text progressText;
		[SerializeField] Text actionText;

		#region Accessors

		int length => panels.Length;
		public bool IsValid => (length > 0);
		
		#endregion

		#region Ops

		public void Begin()
		{
			index = 0;
			inprogress = true;

			root.SetActive(true);
			
			panels[index].Opacity.Show();
			RefreshUI();
			
			opacity.Show();
		}

		public void Next()
		{
			var previousPanel = panels[index];
			previousPanel.Opacity.Hide();
			
			if(index < length-1) 
			{
				panels[++index].Opacity.Show();
				RefreshUI();
			}
			else 
			{
				Complete();
			}
		}

		public void Complete()
		{
			inprogress = false;
			dispose = true;

			StartCoroutine("Dispose");
		}
		
		IEnumerator Dispose()
		{
			opacity.Hide();
			while (opacity.Visible) yield return null;
			root.SetActive(false);

			dispose = false;
		}
		
		#endregion
		
		#region UI

		void RefreshUI()
		{
			UpdateProgressBar();
			UpdateActionText();
		}

		void UpdateProgressBar()
		{
			progressText.text = string.Format("{0} / {1}", index+1, length);
		}
		
		void UpdateActionText()
		{
			bool isLast = (index == length - 1);
			actionText.text = (isLast) ? "Complete" : "Next";
		}
		
		#endregion
	}
}