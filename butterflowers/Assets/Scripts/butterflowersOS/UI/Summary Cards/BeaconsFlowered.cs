using butterflowersOS.Data;

namespace butterflowersOS.UI.Summary_Cards
{
	public class BeaconsFlowered : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.BeaconsFlowered, score.BeaconsFlowered);
		}
	}
}