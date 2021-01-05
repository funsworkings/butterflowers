using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class BeaconsPlanted : ScoreCard
	{
		protected override string Label => "# of beacons removed";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.beaconsPlanted, score.beaconsPlanted);
		}
	}
}