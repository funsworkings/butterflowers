using butterflowersOS.Data;

namespace butterflowersOS.UI.Summary_Cards
{
	public class NestSpills : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.NestSpills, score.NestSpills);
		}
	}
}