using butterflowersOS.Data;

namespace butterflowersOS.UI.Summary_Cards
{
	public class BeaconsAdded : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.BeaconsAdded, score.BeaconsAdded);
		}
	}
}