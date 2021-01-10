using butterflowersOS.Data;

namespace butterflowersOS.UI.Summary_Cards
{
	public class HoB : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.AverageHoB, score.AverageHoB);
		}
	}
}