using System;
using System.Collections;
using Core;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using uwu.Snippets.Load;
using uwu.UI.Behaviors.Visibility;

namespace Objects.Managers
{
	public class SummaryManager : MonoBehaviour, IPauseSun, IReactToSunCycle
	{
		#region Internal

		public enum Panel
		{
			Grades,
			Reload
		}
		
		#endregion

		// Properties

		[SerializeField] GradingManager Grading;

		[SerializeField] ToggleOpacity gamePanel;
		[SerializeField] ToggleOpacity summaryPanel;

		[SerializeField] RawImage photoOfTheDay;
		[SerializeField] TMP_Text photoCaption;

		public bool Pause => active;

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
			var photoRect = photoOfTheDay.rectTransform;
				basePhotoWidth = photoRect.sizeDelta.x;
				basePhotoHeight = photoRect.sizeDelta.y;
			
			HideSummary(); // Hide summary immediately on start
		}

		#endregion
		
		#region Cycle

		public void Cycle(bool refresh)
		{
			if (refresh) 
			{
				ShowSummary();
			}
		}
		
		#endregion
		
		#region Show and hide

		public void ShowSummary()
		{
			m_active = true;
			panel = Panel.Grades;

			StartCoroutine("WaitForSummary");
		}

		IEnumerator WaitForSummary()
		{
			while (Surveillance.Instance.photoInProgress) 
				yield return null;
			
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
			var photoName = Surveillance.Instance.lastPhotoCaption;
			var photo = Surveillance.Instance.lastPhotoTaken;
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