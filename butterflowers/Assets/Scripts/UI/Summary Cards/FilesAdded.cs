using Data;

namespace UI.Summary_Cards
{
	public class FilesAdded : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.filesAdded, score.filesAdded);
		}
	}
}