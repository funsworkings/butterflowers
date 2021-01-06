using Data;

namespace UI.Summary_Cards
{
	public class Discoveries : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.discoveries, score.discoveries);
		}
	}
}