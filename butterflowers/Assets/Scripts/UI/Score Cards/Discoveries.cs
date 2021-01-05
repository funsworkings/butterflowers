using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class Discoveries : ScoreCard
	{
		protected override string Label => "# of discoveries";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.discoveries, score.discoveries);
		}
	}
}