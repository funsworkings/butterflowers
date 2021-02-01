using uwu.Extensions;

namespace butterflowersOS.Data
{
	[System.Serializable]
	public class SurveillanceDataDelta
	{
		public float discoveries = 0f; // Spontaneity, play

		public float hob = 0f; // Destruction, play
		public float nestfill = 0f; // Nurture, destruction, gluttony

		public float beaconsAdded = 0f; // Gluttony, destruction
		public float beaconsPlanted = 0f; // Nurture, rest

		public float nestKicks = 0f; // Play, rest
		public float nestSpills = 0f; // Destruction, gluttonyttony

		public float MAX_DELTA;
		
		
		public SurveillanceDataDelta(CompositeSurveillanceData a, CompositeSurveillanceData b)
		{
			float MD = 0F;
			
			discoveries = Extensions.PercentageDifferenceWithMax(a.Discoveries, b.Discoveries, ref MD);

			hob = Extensions.PercentageDifferenceWithMax(a.AverageHoB, b.AverageHoB, ref MD);
			nestfill = Extensions.PercentageDifferenceWithMax(a.AverageNestFill, b.AverageNestFill, ref MD);
			
			beaconsAdded = Extensions.PercentageDifferenceWithMax(a.BeaconsAdded, b.BeaconsAdded, ref MD);
			beaconsPlanted = Extensions.PercentageDifferenceWithMax(a.BeaconsPlanted, b.BeaconsPlanted, ref MD);
			nestKicks = Extensions.PercentageDifferenceWithMax(a.NestKicks, b.NestKicks, ref MD);
			nestSpills = Extensions.PercentageDifferenceWithMax(a.NestSpills, b.NestSpills, ref MD);

			MAX_DELTA = MD;
		}
	}
}