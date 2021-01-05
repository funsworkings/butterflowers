using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class BeaconsAdded : ScoreCard
	{
		protected override string Label => "# of beacons added";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.beaconsAdded, score.beaconsAdded);
		}
	}
}