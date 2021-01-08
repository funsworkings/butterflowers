using Data;

namespace UI.Summary_Cards
{
	public class BeaconsDestroyed : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.BeaconsDestroyed, score.BeaconsDestroyed);
		}
	}
}