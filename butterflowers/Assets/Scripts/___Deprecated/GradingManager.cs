using System;
using System.Collections;
using System.Linq;
using butterflowersOS.Core;
using butterflowersOS.Data;
using butterflowersOS.Presets;
using butterflowersOS.UI;
using butterflowersOS.UI.Summary_Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uwu.UI.Behaviors.Visibility;

namespace Objects.Managers
{
	public class GradingManager : MonoBehaviour
	{
		// External

		[SerializeField] Surveillance Surveillance = null;
		
		// Properties

		[HideInInspector] public bool inprogress = false;

		[SerializeField] WorldPreset Preset;
		[SerializeField] Animator animator;
		
		[SerializeField] TMP_Text score;

		[SerializeField] Transform scoreItemParent;
		[SerializeField] SummaryDeck deck = null;

		[SerializeField] ToggleScale scoreScaler;

		[SerializeField] Image strokeImage;
		[SerializeField] Animation scoreDropdownAnimation;


		#region Monobehaviour callbacks

		void Start()
		{
			HideScores();
		}

		#endregion

		#region Operations

		[SerializeField] CompositeSurveillanceData current, average;

		public void ShowScores()
		{
			var log = current = new CompositeSurveillanceData(Surveillance.activeLog);
			var compositeLog = average = Surveillance.CreateCompositeAverageLog();

			foreach (SummaryCard card in deck.Items) 
			{
				ShowScoreItem(card, compositeLog, log);
			}

			inprogress = true;
			StartCoroutine("DisplayDeck");

			/*****/
			//char grade = CalculateGrade(log, compositeLog);
			//score.text = grade.ToString();
			/*****/
		}

		IEnumerator DisplayDeck()
		{
			deck.Close(immediate:true);
			yield return new WaitForEndOfFrame();
			
			deck.Drop();
			yield return new WaitForSecondsRealtime(1f);
			deck.Open();
		}

		public void HideScores()
		{
			StopAllCoroutines();
			StartCoroutine("DisposeDeck");
		}

		IEnumerator DisposeDeck()
		{
			deck.Close();
			while (deck.inprogress || deck._State != SummaryDeck.State.Disabled) yield return null;
			
			inprogress = false;
		}

		void ShowScoreItem(SummaryCard card, CompositeSurveillanceData average, CompositeSurveillanceData current)
		{
			if (!(card is PhotoOfTheDay)) 
			{
				card.ShowScore(average, current);
			}
		}

		void HideScoreItem(SummaryCard card)
		{
			card.HideScore();
		}

		#endregion
		
		#region Scoring

		public char CalculateGrade(CompositeSurveillanceData current, CompositeSurveillanceData composite)
		{
			/*
			float fileadd = CalculatePercentageIncrease(composite.filesAdded, current.filesAdded);
			float fileremove = -CalculatePercentageIncrease(composite.filesRemoved, current.filesRemoved);
			float discovery = CalculatePercentageIncrease(composite.discoveries, current.discoveries);

			float beaconadd = CalculatePercentageIncrease(composite.beaconsAdded, current.beaconsAdded);
			float beaconplant = CalculatePercentageIncrease(composite.beaconsPlanted, current.beaconsPlanted);

			float nestkick = CalculatePercentageIncrease(composite.nestKicks, current.nestKicks);
			float nestspill = CalculatePercentageIncrease(composite.nestSpills, current.nestSpills);
			
			
			scores = new float[]{ fileadd, fileremove, discovery, beaconadd, beaconplant, nestkick, nestspill };
			float averagePercentIncrease = scores.Average();

			if (averagePercentIncrease >= .5f)
				return 'A';
			else if (averagePercentIncrease >= .3f)
				return 'B';
			else if (averagePercentIncrease >= .125f)
				return 'C';
			else if (averagePercentIncrease >= 0f)
				return 'D';
			else
				return 'F';
				*/

			return 'A';
		}

		float CalculatePercentageIncrease(float a, float b)
		{
			if (a == 0f)
				a = .001f; // Turn into small value
			
			return Mathf.Clamp((b - a)/a, -1f, 1f);
		}
		
		#endregion
		
		#region Deck callbacks

		void DeckDidOpen()
		{
			
		}
		
		void DeckDidClose()
		{
			
		}
		
		#endregion
	}
}