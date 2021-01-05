using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class FilesRemoved : ScoreCard
	{
		protected override string Label => "# of files removed";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.filesRemoved, score.filesRemoved);
		}
	}
}