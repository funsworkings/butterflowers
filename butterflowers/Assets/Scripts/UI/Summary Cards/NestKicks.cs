using Data;

namespace UI.Summary_Cards
{
	public class NestKicks : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.NestKicks, score.NestKicks);
		}
	}
}