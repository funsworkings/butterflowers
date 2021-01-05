using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class NestKicks : ScoreCard
	{
		protected override string Label => "# of nest kicks";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.nestKicks, score.nestKicks);
		}
	}
}