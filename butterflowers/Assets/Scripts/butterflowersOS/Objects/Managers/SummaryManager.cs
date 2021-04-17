using System.Collections;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.UI;
using butterflowersOS.UI.Summary_Cards;
using Neue.Agent.Brain.Data;
using Objects.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Objects.Managers
{
	public class SummaryManager : MonoBehaviour, IPauseSun
	{
		// EVents

		public UnityEvent onOpen, onClose;

		// Properties

		[SerializeField] SummaryPanel[] _panels = new SummaryPanel[]{};
		[SerializeField] Animator summaryPanel = null;

		[SerializeField] PhotoOfTheDay photoOfTheDay = null;
		[SerializeField] TMP_Text photoCaption;

		public bool Pause => active;

		// Attributes

		[SerializeField] bool m_active = false;
		[SerializeField] int panelIndex = 0;

		#region Accessors

		public bool active => m_active;
		
		#endregion
		
		#region Monobehaviour callbacks

		void Start()
		{
			HideSummary(); // Hide summary immediately on start
		}

		#endregion
		
		#region Movement

		public void NextPanel()
		{
			if (panelIndex > -1 && panelIndex < _panels.Length-1) 
			{
				var cachePanel = _panels[panelIndex];
				cachePanel.Hide();
			}

			if (++panelIndex >= _panels.Length) 
			{
				HideSummary();	
			}
			else 
			{
				_panels[panelIndex].Show();
			}
		}
		
		#endregion

		#region Show and hide

		public void ShowSummary()
		{
			m_active = true;
			panelIndex = 0;
			
			onOpen.Invoke();
			
			foreach(SummaryPanel p in _panels) p.Hide();

			StartCoroutine("Show");
		}

		IEnumerator Show()
		{
			while (Surveillance.Instance.photoInProgress) yield return null;
			SetPhotoOfTheDay();

			panelIndex = -1;
			NextPanel();
			
			summaryPanel.SetBool("visible", true);
		}

		public void HideSummary()
		{
			StartCoroutine("Hide");
		}

		IEnumerator Hide()
		{
			summaryPanel.SetBool("visible", false);
			yield return null;
			
			m_active = false;
			
			onClose.Invoke();
		}

		#endregion
		
		#region Photo of the day

		public void SetPhotoOfTheDay()
		{
			var photoName = Surveillance.Instance.lastPhotoCaption;
			var photo = Surveillance.Instance.lastPhotoTaken;
			var valid = photo != null;

			if (valid) 
			{
				string caption = string.Format("{0}.jpg", photoName);
				photoOfTheDay.ShowPhoto(photo, caption);
			}
		}
		
		#endregion
		
	}
}