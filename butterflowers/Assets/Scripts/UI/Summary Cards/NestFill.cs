using Data;

namespace UI.Summary_Cards
{
	public class NestFill : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.AverageNestFill, score.AverageNestFill);
		}
	}
}