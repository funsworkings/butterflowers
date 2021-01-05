using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class HoB : ScoreCard
	{
		protected override string Label => "butterflowers";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.AverageHoB, score.averageHoB);
		}
	}
}