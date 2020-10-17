using System;
using System.Collections;
using System.Linq;
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

		[SerializeField] TMP_Text score;

		[SerializeField] Transform scoreItemParent;
		ScoreItem[] scoreItems;

		[SerializeField] ScoreItem addedFiles;
		[SerializeField] ScoreItem removedFiles;
		[SerializeField] ScoreItem discoveries;
		[SerializeField] ScoreItem healthOfButterflies;
		[SerializeField] ScoreItem beaconsPlanted;
		[SerializeField] ScoreItem beaconsAdded;
		[SerializeField] ScoreItem cursorVelocity;
		[SerializeField] ScoreItem timeInNest;
		[SerializeField] ScoreItem timeInTree;
		[SerializeField] ScoreItem timeInMagicStar;
		[SerializeField] ScoreItem timeInDefault;
		[SerializeField] ScoreItem nestFill;
		[SerializeField] ScoreItem nestKicks;
		[SerializeField] ScoreItem nestSpills;

		[SerializeField] ToggleScale scoreScaler;

		[SerializeField] Image strokeImage;
		[SerializeField] Animation scoreDropdownAnimation;
		
		// Attributes

		[SerializeField] float scoreLerpSpeed = 1f;
		[SerializeField] float timeBetweenScores = .1f;
		[SerializeField] float timeBeforeStroke = .3f;
		
		[Header("Debug")]
			[SerializeField] float[] scores = new float[]{};
			[SerializeField] ScoringData composite;
			[SerializeField] bool refreshComposite = false;
			
			
		#region Monobehaviour callbacks

		void Start()
		{
			scoreItems = scoreItemParent.GetComponentsInChildren<ScoreItem>();
			HideScores();
		}
		
		void Update()
		{
			if (refreshComposite) {
				composite = CreateCompositeAverageLog();
				refreshComposite = false;
			}
		}
		
		#endregion

		#region Operations

		public void ShowScores()
		{
			var log = new ScoringData(Surveillance.activeLog);
			var compositeLog = CreateCompositeAverageLog();

			StartCoroutine(ShowingScores(compositeLog, log));
			
			/*****/
				char grade = CalculateGrade(log, compositeLog);
				score.text = grade.ToString();
			/*****/
		}

		IEnumerator ShowingScores(ScoringData compositeLog, ScoringData log)
		{
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(addedFiles, compositeLog.filesAdded,log.filesAdded);
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(removedFiles,compositeLog.filesRemoved,log.filesRemoved);
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(discoveries, compositeLog.discoveries,log.discoveries);
			
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(healthOfButterflies,compositeLog.AverageHoB,log.averageHoB);
			
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(beaconsAdded,compositeLog.beaconsAdded,log.beaconsAdded);
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(beaconsPlanted,compositeLog.beaconsPlanted,log.beaconsPlanted);
			
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(nestFill,compositeLog.AverageNestFill,log.averageNestFill);
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(nestKicks,compositeLog.nestKicks,log.nestKicks);
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(nestSpills,compositeLog.nestSpills,log.nestSpills);
			
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(timeInNest,compositeLog.AverageTimeSpentInNest, log.timeSpentInNest);
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(timeInTree,compositeLog.AverageTimeSpentInTree,log.timeSpentInTree);
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(timeInMagicStar,compositeLog.AverageTimeSpentInMagicStar,log.timeSpentInMagicStar);
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(timeInDefault,compositeLog.AverageTimeSpentInDefault,log.timeSpentInDefault);
			
			yield return new WaitForSeconds(timeBetweenScores);
			ShowScoreItem(cursorVelocity,compositeLog.AverageCursorSpeed,log.averageCursorSpeed);
			
			yield return new WaitForSeconds(timeBetweenScores*2f);
			scoreScaler.Show();

			while (!scoreScaler.Shown) yield return null;
			yield return new WaitForSeconds(timeBeforeStroke);
			scoreDropdownAnimation.Play();
		}

		public void HideScores()
		{
			StopAllCoroutines();
			foreach(ScoreItem item in scoreItems)
				item.gameObject.SetActive(false);
			
			scoreScaler.Hide();
			strokeImage.fillAmount = 0f;
		}

		void ShowScoreItem(ScoreItem item, float previousScore, float currentScore)
		{
			item.gameObject.SetActive(true);
			item.SetScore(previousScore, currentScore, scoreLerpSpeed);
		}

		#endregion
		
		#region Scoring

		public char CalculateGrade(ScoringData current, ScoringData composite)
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

		ScoringData CreateCompositeAverageLog()
		{
			var logs = Surveillance.previousLogs;
			if (logs == null)
				return Preset.baselineSurveillanceData;

			ScoringData composite = new ScoringData();
				
				composite.filesAdded = (int) logs.Select(l => l.filesAdded).Average();
				composite.filesRemoved = (int) logs.Select(l => l.filesRemoved).Average();
				composite.discoveries = (int)logs.Select(l => l.discoveries).Average();
				
				composite.beaconsAdded = (int) logs.Select(l => l.beaconsAdded).Average();
				composite.beaconsPlanted = (int)logs.Select(l => l.beaconsPlanted).Average();

				composite.nestKicks = (int) logs.Select(l => l.nestKicks).Average();
				composite.nestSpills = (int)logs.Select(l => l.nestSpills).Average();

				composite.AverageCursorSpeed = logs.Select(l => l.averageCursorSpeed).Average();
				composite.AverageHoB = logs.Select(l => l.averageHoB).Average();
				composite.AverageNestFill = logs.Select(l => l.averageNestFill).Average();

				composite.AverageTimeSpentInNest = logs.Select(l => l.timeSpentInNest).Average();
				composite.AverageTimeSpentInTree = logs.Select(l => l.timeSpentInTree).Average();
				composite.AverageTimeSpentInMagicStar = logs.Select(l => l.timeSpentInMagicStar).Average();
				composite.AverageTimeSpentInDefault = logs.Select(l => l.timeSpentInDefault).Average();
				

			return composite;
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