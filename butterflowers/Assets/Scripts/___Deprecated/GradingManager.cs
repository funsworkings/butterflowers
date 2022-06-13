using System.Collections;
using butterflowersOS.Core;
using butterflowersOS.Data;
using butterflowersOS.Presets;
using butterflowersOS.UI;
using butterflowersOS.UI.Summary_Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Objects.Managers
{
	public class GradingManager : MonoBehaviour
	{
		// External

		[SerializeField] Surveillance Surveillance = null;
		
		// Properties

		[HideInInspector] public bool inprogress = false;
		[SerializeField] SummaryDeck deck = null;


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
		}

		IEnumerator DisplayDeck()
		{
			deck.Close(immediate:true);
			yield return new WaitForEndOfFrame();

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
	}
}