using Data;

namespace UI.Summary_Cards
{
	public class HoB : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.AverageHoB, score.AverageHoB);
		}
	}
}