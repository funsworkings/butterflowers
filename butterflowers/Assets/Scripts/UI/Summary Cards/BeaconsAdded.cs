using Data;

namespace UI.Summary_Cards
{
	public class BeaconsAdded : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.beaconsAdded, score.beaconsAdded);
		}
	}
}