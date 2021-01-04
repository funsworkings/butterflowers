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
		ScoreCard[] scoreItems;

		[SerializeField] ScoreCard addedFiles;
		[SerializeField] ScoreCard removedFiles;
		[SerializeField] ScoreCard discoveries;
		[SerializeField] ScoreCard healthOfButterflies;
		[SerializeField] ScoreCard beaconsPlanted;
		[SerializeField] ScoreCard beaconsAdded;
		[SerializeField] ScoreCard cursorVelocity;
		[SerializeField] ScoreCard timeInNest;
		[SerializeField] ScoreCard timeInTree;
		[SerializeField] ScoreCard timeInMagicStar;
		[SerializeField] ScoreCard timeInDefault;
		[SerializeField] ScoreCard nestFill;
		[SerializeField] ScoreCard nestKicks;
		[SerializeField] ScoreCard nestSpills;

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
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(addedFiles, compositeLog.filesAdded,log.filesAdded);
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(removedFiles,compositeLog.filesRemoved,log.filesRemoved);
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(discoveries, compositeLog.discoveries,log.discoveries);
			
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(healthOfButterflies,compositeLog.AverageHoB,log.averageHoB);
			
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(beaconsAdded,compositeLog.beaconsAdded,log.beaconsAdded);
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(beaconsPlanted,compositeLog.beaconsPlanted,log.beaconsPlanted);
			
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(nestFill,compositeLog.AverageNestFill,log.averageNestFill);
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(nestKicks,compositeLog.nestKicks,log.nestKicks);
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(nestSpills,compositeLog.nestSpills,log.nestSpills);
			
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(timeInNest,compositeLog.AverageTimeSpentInNest, log.timeSpentInNest);
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(timeInTree,compositeLog.AverageTimeSpentInTree,log.timeSpentInTree);
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(timeInMagicStar,compositeLog.AverageTimeSpentInMagicStar,log.timeSpentInMagicStar);
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(timeInDefault,compositeLog.AverageTimeSpentInDefault,log.timeSpentInDefault);
			
			yield return new WaitForSecondsRealtime(timeBetweenScores);
			ShowScoreItem(cursorVelocity, compositeLog.AverageCursorSpeed, log.averageCursorSpeed);
		}

		public void HideScores()
		{
			StopAllCoroutines();
			foreach (ScoreCard item in scoreItems) 
			{
				HideScoreItem(item);
			}
		}

		void ShowScoreItem(ScoreCard card, float previousScore, float currentScore)
		{
			card.gameObject.SetActive(true);
			card.ShowScore(previousScore, currentScore);
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