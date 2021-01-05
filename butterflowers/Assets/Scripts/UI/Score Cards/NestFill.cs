using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class NestFill : ScoreCard
	{
		protected override string Label => "nest fill";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.AverageNestFill, score.averageNestFill);
		}
	}
}