using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class CursorSpeed : ScoreCard
	{
		protected override string Label => "wand speed";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.AverageCursorSpeed, score.averageCursorSpeed);
		}
	}
}