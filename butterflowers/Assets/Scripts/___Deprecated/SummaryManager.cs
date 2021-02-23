using System;
using System.Collections;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.UI.Summary_Cards;
using Neue.Agent.Brain.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.UI;
using uwu.Snippets.Load;
using uwu.UI.Behaviors.Visibility;

namespace Objects.Managers
{
	public class SummaryManager : MonoBehaviour, IPauseSun
	{
		#region Internal

		public enum Panel
		{
			Grades,
			Reload
		}
		
		#endregion
		
		// EVents

		public UnityEvent onOpen, onClose;

		// Properties

		[SerializeField] GradingManager Grading = null;
		
		[SerializeField] ToggleOpacity summaryPanel = null;

		[SerializeField] PhotoOfTheDay photoOfTheDay = null;
		[SerializeField] TMP_Text photoCaption;

		public bool Pause => active;

		// Attributes

		[SerializeField] bool m_active = false;
		[SerializeField] Panel panel = Panel.Grades;

		#region Accessors

		public bool active => m_active;
		public Panel ActivePanel => panel;
		
		#endregion
		
		#region Monobehaviour callbacks

		void Start()
		{
			/*var photoRect = photoOfTheDay.rectTransform;
				basePhotoWidth = photoRect.sizeDelta.x;
				basePhotoHeight = photoRect.sizeDelta.y;
			*/
			
			HideSummary(); // Hide summary immediately on start
		}

		#endregion

		#region Show and hide

		public void ShowSummary(Profile profile = null)
		{
			m_active = true;
			panel = Panel.Grades;
			
			onOpen.Invoke();

			StartCoroutine("Show");
		}

		IEnumerator Show()
		{
			while (Surveillance.Instance.photoInProgress) yield return null;
			
			DisplayPhotoOfTheDay();
			Grading.ShowScores();
			
			summaryPanel.Show();
		}

		public void HideSummary()
		{
			StartCoroutine("Hide");
		}

		IEnumerator Hide()
		{
			Grading.HideScores();
			while (Grading.inprogress) yield return null;

			summaryPanel.Hide();
			while (summaryPanel.Visible) yield return null;
			
			m_active = false;
			
			onClose.Invoke();
		}

		#endregion
		
		#region Photo of the day

		public void DisplayPhotoOfTheDay()
		{
			var photoName = Surveillance.Instance.lastPhotoCaption;
			var photo = Surveillance.Instance.lastPhotoTaken;
			var valid = photo != null;

			//photoOfTheDay.gameObject.SetActive(valid);
			if (valid) 
			{
				/*
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
				*/
				
				string caption = string.Format("{0}.jpg", photoName);
				photoOfTheDay.ShowPhoto(photo, caption);
			}
		}
		
		#endregion
		
	}
}