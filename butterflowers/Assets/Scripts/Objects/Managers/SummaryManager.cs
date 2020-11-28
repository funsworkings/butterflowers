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
		Nest nest;
		
		// Properties

		[SerializeField] GradingManager Grading;

		[SerializeField] ToggleOpacity gamePanel;
		[SerializeField] ToggleOpacity summaryPanel;

		[SerializeField] RawImage photoOfTheDay;
		[SerializeField] TMP_Text photoCaption;

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
			nest = Nest.Instance;
			
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
			Grading.ShowScores();
			
			gamePanel.Hide();
			summaryPanel.Show();
		}

		public void HideSummary()
		{
			summaryPanel.Hide();
			gamePanel.Show();
			
			Grading.HideScores();

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
		}
		
		#endregion
		
	}
}