using Data;

namespace UI.Summary_Cards
{
	public class FilesRemoved : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.FilesRemoved, score.FilesRemoved);
		}
	}
}