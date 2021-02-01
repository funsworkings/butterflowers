using butterflowersOS.Presets;
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
		public float beaconsFlowered = 0f;
		public float beaconsDestroyed = 0f;

		public float nestKicks = 0f; // Play, rest
		public float nestSpills = 0f; // Destruction, gluttonyttony

		public float events = 0f; // Quiet
		
		
		public SurveillanceDataDelta(CompositeSurveillanceData a, CompositeSurveillanceData b, WorldPreset preset)
		{
			float totalEvents = 0f;
			
			discoveries = preset.discoveryScoreCurve.Evaluate(b.Discoveries); totalEvents += b.Discoveries;

			hob = b.AverageHoB; totalEvents += b.AverageHoB;
			nestfill = b.AverageNestFill; totalEvents += b.AverageNestFill;
			
			beaconsAdded = preset.beaconsAddScoreCurve.Evaluate(b.BeaconsAdded); totalEvents += b.BeaconsAdded;
			beaconsPlanted = preset.beaconsPlantScoreCurve.Evaluate(b.BeaconsPlanted); totalEvents += b.BeaconsPlanted;
			beaconsDestroyed = preset.beaconsDestroyScoreCurve.Evaluate(b.BeaconsDestroyed); totalEvents += b.BeaconsDestroyed;
			beaconsFlowered = preset.beaconsFlowerScoreCurve.Evaluate(b.BeaconsFlowered); totalEvents += b.BeaconsFlowered;
			
			nestKicks = preset.nestKickScoreCurve.Evaluate(b.NestKicks); totalEvents += b.NestKicks;
			nestSpills = preset.nestSpillScoreCurve.Evaluate(b.NestSpills); totalEvents += b.NestSpills;
			
			events = preset.eventStackScoreCurve.Evaluate(totalEvents / preset.eventStackHeight);
		}
	}
}