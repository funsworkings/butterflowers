using UnityEngine;
using uwu.Extensions;

namespace Data
{
	[System.Serializable]
	public class SurveillanceDataDelta
	{
		public float filesAdded = 0f; // Gluttony, nurture
		public float filesRemoved = 0f; // Destruction, spontaneity
		public float discoveries = 0f; // Spontaneity, play

		public float hob = 0f; // Destruction, play
		public float nestfill = 0f; // Nurture, destruction, gluttony
		public float cursorspeed = 0f; // Play, spontaneity

		// Rest, spontaneity, play
		public float volatility = 0f;

		public float beaconsAdded = 0f; // Gluttony, destruction
		public float beaconsPlanted = 0f; // Nurture, rest

		public float nestKicks = 0f; // Play, rest
		public float nestSpills = 0f; // Destruction, gluttonyttony
		
		
		public SurveillanceDataDelta(CompositeSurveillanceData a, CompositeSurveillanceData b)
		{
			filesAdded = Extensions.PercentageDifference(a.filesAdded, b.filesAdded);
			filesRemoved = Extensions.PercentageDifference(a.filesRemoved, b.filesRemoved);
			discoveries = Extensions.PercentageDifference(a.discoveries, b.discoveries);

			hob = Extensions.PercentageDifference(a.AverageHoB, b.AverageHoB);
			nestfill = Extensions.PercentageDifference(a.AverageNestFill, b.AverageNestFill);
			cursorspeed = Extensions.PercentageDifference(a.AverageCursorSpeed, b.AverageCursorSpeed);

			volatility = Extensions.PercentageDifference(a.CalculateFocusVolatility(), b.CalculateFocusVolatility());

			beaconsAdded = Extensions.PercentageDifference(a.beaconsAdded, b.beaconsAdded);
			beaconsPlanted = Extensions.PercentageDifference(a.beaconsPlanted, b.beaconsPlanted);

			nestKicks = Extensions.PercentageDifference(a.nestKicks, b.nestKicks);
			nestSpills = Extensions.PercentageDifference(a.nestSpills, b.nestSpills);
		}
	}
}