﻿using butterflowersOS.Data;

namespace butterflowersOS.UI.Summary_Cards
{
	public class BeaconsPlanted : SummaryCard
	{
		public override void ShowScore(CompositeSurveillanceData average, CompositeSurveillanceData score)
		{
			ShowScore(average.BeaconsPlanted, score.BeaconsPlanted);
		}
	}
}