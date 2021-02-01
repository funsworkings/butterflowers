using uwu.Extensions;

namespace butterflowersOS.Data
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
			filesAdded = Extensions.PercentageDifference(a.FilesAdded, b.FilesAdded);
			filesRemoved = Extensions.PercentageDifference(a.FilesRemoved, b.FilesRemoved);
			discoveries = Extensions.PercentageDifference(a.Discoveries, b.Discoveries);

			hob = Extensions.PercentageDifference(a.AverageHoB, b.AverageHoB);
			nestfill = Extensions.PercentageDifference(a.AverageNestFill, b.AverageNestFill);
			cursorspeed = Extensions.PercentageDifference(a.AverageCursorSpeed, b.AverageCursorSpeed);

			volatility = Extensions.PercentageDifference(a.CalculateFocusVolatility(), b.CalculateFocusVolatility());

			beaconsAdded = Extensions.PercentageDifference(a.BeaconsAdded, b.BeaconsAdded);
			beaconsPlanted = Extensions.PercentageDifference(a.BeaconsPlanted, b.BeaconsPlanted);
			nestKicks = Extensions.PercentageDifference(a.NestKicks, b.NestKicks);
			nestSpills = Extensions.PercentageDifference(a.NestSpills, b.NestSpills);
		}
	}
}