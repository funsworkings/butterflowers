using System;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;
using uwu.Snippets.Load;
using uwu.UI.Behaviors.Visibility;

namespace Objects.Managers
{
	public class SummaryManager : MonoBehaviour
	{
		#region Internal

		public enum Panel
		{
			Grades,
			Reload
		}
		
		#endregion
		
		// External

		World World;
		
		// Properties

		[SerializeField] GradingManager Grading;
		[SerializeField] LoadingManager Loading;

		[SerializeField] ToggleOpacity gamePanel;
		[SerializeField] ToggleOpacity summaryPanel;
		[SerializeField] ToggleOpacity gradingPanel;
		[SerializeField]  ToggleOpacity loadingPanel;
		[SerializeField] ToggleOpacity escapeButton;

		[SerializeField] RawImage photoOfTheDay;
		[SerializeField] TMP_Text photoCaption;
		[SerializeField] ToggleScale photoScale;
		
		// Attributes

		[SerializeField] bool m_active = false;
		[SerializeField] Panel panel = Panel.Grades;

		float basePhotoWidth = -1f, basePhotoHeight = -1f;
		
		#region Accessors

		public bool active => m_active;
		public Panel ActivePanel => panel;
		
		#endregion
		
		#region Monobehaviour callbacks

		void Start()
		{
			World = World.Instance;
			
			var photoRect = photoOfTheDay.rectTransform;
				basePhotoWidth = photoRect.sizeDelta.x;
				basePhotoHeight = photoRect.sizeDelta.y;
			
			HideSummary(); // Hide summary immediately on start
		}

		#endregion
		
		#region Show and hide

		public void ShowSummary()
		{
			m_active = true;
			panel = Panel.Grades;
			
			DisplayPhotoOfTheDay();
			
			gamePanel.Hide();
			summaryPanel.Show();
			gradingPanel.Show();
			loadingPanel.Hide();
			escapeButton.Hide();
			
			Grading.ShowScores();
		}
		
		public void MoveToLoading()
		{
			photoScale.Hide();
			
			Loading.ShowLoading();
			Grading.HideScores();
			
			gradingPanel.Hide();
			loadingPanel.Show();
			escapeButton.Hide();

			panel = Panel.Reload;
		}

		public void ShowEscape()
		{
			escapeButton.Show();
		}

		public void HideSummary()
		{
			summaryPanel.Hide();
			gradingPanel.Hide();
			loadingPanel.Hide();
			escapeButton.Hide();
			
			photoScale.Hide();

			gamePanel.Show();

			m_active = false;
			panel = Panel.Grades;
		}

		#endregion
		
		#region Photo of the day

		public void DisplayPhotoOfTheDay()
		{
			var photoName = World.LastPhotoCaption;
			var photo = World.LastPhoto;
			var valid = photo != null;

			photoOfTheDay.enabled = valid;
			if (valid) {
				var w = (float)photo.width;
				var h = (float)photo.height;
				var aspect = 1f;
				
				if (w > h) {
					aspect = (h / w);
					
					w = basePhotoWidth;
					h = basePhotoWidth * aspect;
				}
				else {
					aspect = (w / h);
					
					h = basePhotoHeight;
					w = basePhotoHeight * aspect;
				}

				photoOfTheDay.rectTransform.sizeDelta = new Vector2(w, h);
				photoOfTheDay.texture = photo;
			}

			photoCaption.text = string.Format("{0}.jpg", photoName);
			photoScale.Show();
		}
		
		#endregion
		
	}
}