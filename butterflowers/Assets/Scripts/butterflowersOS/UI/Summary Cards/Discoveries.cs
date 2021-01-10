using butterflowersOS.Data;

namespace butterflowersOS.UI.Summary_Cards
{
	public class Discoveries : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.Discoveries, score.Discoveries);
		}
	}
}