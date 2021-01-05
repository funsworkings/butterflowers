using Data;
using UnityEngine;

namespace UI.Score_Cards
{
	public class FilesAdded : ScoreCard
	{
		protected override string Label => "# of files added";
		
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.filesAdded, score.filesAdded);
		}
	}
}