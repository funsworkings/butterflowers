using System;
using System.Collections;
using butterflowersOS.Core;
using butterflowersOS.Data;
using butterflowersOS.UI.Summary_Cards;
using UnityEngine;

namespace butterflowersOS.UI.Summary_Panels
{
	public class HighlightsPanel : SummaryPanel
	{
		// Properties

		SummaryDeck deck;
		Surveillance Surveillance = null;

		protected override void Start()
		{
			base.Start();
			
			deck = GetComponentInChildren<SummaryDeck>();
			Surveillance = FindObjectOfType<Surveillance>();
		}

		protected override void OnShown()
		{
			SendHighlightsToCards();
		}

		protected override void OnHidden()
		{
			StopCoroutine("DisplayDeck");
		}

		void SendHighlightsToCards()
		{
			var log = new CompositeSurveillanceData(Surveillance.activeLog);
			var compositeLog = Surveillance.CreateCompositeAverageLog();
			
			foreach (SummaryCard card in deck.Items) 
			{
				if (!(card is PhotoOfTheDay)) 
				{
					card.ShowScore(compositeLog, log);
				}
			}

			StartCoroutine("DisplayDeck");
		}
		

		IEnumerator DisplayDeck()
		{
			deck.Close(immediate:true);
			yield return new WaitForEndOfFrame();
			
			deck.Open();
		}
	}
}