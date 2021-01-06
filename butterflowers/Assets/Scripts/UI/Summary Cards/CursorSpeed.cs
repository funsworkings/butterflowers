using Data;

namespace UI.Summary_Cards
{
	public class CursorSpeed : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.AverageCursorSpeed, score.averageCursorSpeed);
		}
	}
}