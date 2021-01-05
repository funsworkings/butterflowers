using System;
using System.Collections;
using System.Linq;
using Core;
using Data;
using Settings;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using uwu.UI.Behaviors.Visibility;

namespace Objects.Managers
{
	public class GradingManager : MonoBehaviour
	{
		// External

		[SerializeField] Surveillance Surveillance;
		
		// Properties

		[SerializeField] WorldPreset Preset;
		[SerializeField] Animator animator;
		
		[SerializeField] TMP_Text score;

		[SerializeField] Transform scoreItemParent;
		[SerializeField] ScoreCard[] scoreItems;

		[SerializeField] ToggleScale scoreScaler;

		[SerializeField] Image strokeImage;
		[SerializeField] Animation scoreDropdownAnimation;
		
		// Attributes
		
		[SerializeField] float timeBetweenScores = .1f;
		[SerializeField] float timeBeforeStroke = .3f;
		
		[Header("Debug")]
			[SerializeField] float[] scores = new float[]{};
			[SerializeField] CompositeSurveillanceData composite;
			[SerializeField] bool refreshComposite = false;
			
			
		#region Monobehaviour callbacks

		void Start()
		{
			scoreItems = scoreItemParent.GetComponentsInChildren<ScoreCard>();
			HideScores();
		}

		#endregion

		#region Operations

		public void ShowScores()
		{
			var log = new CompositeSurveillanceData(Surveillance.activeLog);
			var compositeLog = Surveillance.CreateCompositeAverageLog();

			StartCoroutine(ShowingScores(compositeLog, log));
			
			/*****/
				//char grade = CalculateGrade(log, compositeLog);
				//score.text = grade.ToString();
			/*****/
		}

		IEnumerator ShowingScores(CompositeSurveillanceData compositeLog, CompositeSurveillanceData log)
		{
			foreach (ScoreCard card in scoreItems) 
			{
				yield return new WaitForSecondsRealtime(timeBetweenScores);
				ShowScoreItem(card, compositeLog, log);
			}
		}

		public void HideScores()
		{
			StopAllCoroutines();
			foreach (ScoreCard item in scoreItems) 
			{
				HideScoreItem(item);
			}
		}

		void ShowScoreItem(ScoreCard card, CompositeSurveillanceData average, CompositeSurveillanceData current)
		{
			card.gameObject.SetActive(true);
			card.ShowScore(average, current);
		}

		void HideScoreItem(ScoreCard card)
		{
			card.HideScore();
			card.gameObject.SetActive(false);
		}

		#endregion
		
		#region Scoring

		public char CalculateGrade(CompositeSurveillanceData current, CompositeSurveillanceData composite)
		{
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
		}

		float CalculatePercentageIncrease(float a, float b)
		{
			if (a == 0f)
				a = .001f; // Turn into small value
			
			return Mathf.Clamp((b - a)/a, -1f, 1f);
		}
		
		#endregion
	}
}