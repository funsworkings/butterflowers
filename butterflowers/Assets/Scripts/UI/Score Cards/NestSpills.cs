using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class NestSpills : ScoreCard
	{
		protected override string Label => "# of nest spills";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.nestSpills, score.nestSpills);
		}
	}
}