using Data;

namespace UI.Summary_Cards
{
	public class NestSpills : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.nestSpills, score.nestSpills);
		}
	}
}