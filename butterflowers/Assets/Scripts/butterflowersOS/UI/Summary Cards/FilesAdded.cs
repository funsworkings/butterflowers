using butterflowersOS.Data;

namespace butterflowersOS.UI.Summary_Cards
{
	public class FilesAdded : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.FilesAdded, score.FilesAdded);
		}
	}
}