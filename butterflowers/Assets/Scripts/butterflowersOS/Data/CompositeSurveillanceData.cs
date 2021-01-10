using uwu.Extensions;

namespace butterflowersOS.Data
{
	[System.Serializable]
	public class CompositeSurveillanceData : SurveillanceData
	{
		public float AverageHoB = 0f;
		public float AverageNestFill = 0f;
		public float AverageCursorSpeed = 0f;

		public float AverageTimeSpentInNest = 0f;
		public float AverageTimeSpentInMagicStar = 0f;
		public float AverageTimeSpentInTree = 0f;
		public float AverageTimeSpentInDefault = 0f;

		public int FilesAdded = 0;
		public int FilesRemoved = 0;
		public int Discoveries = 0;
		public int BeaconsAdded = 0;
		public int BeaconsDestroyed = 0;
		public int BeaconsPlanted = 0;
		public int NestKicks = 0;
		public int NestSpills = 0;

		public CompositeSurveillanceData(){}
		
		public CompositeSurveillanceData(SurveillanceData dat)
		{
			AverageHoB = dat.averageHoB;
			AverageNestFill = dat.averageNestFill;
			AverageCursorSpeed = dat.averageCursorSpeed;

			AverageTimeSpentInNest = dat.timeSpentInNest;
			AverageTimeSpentInMagicStar = dat.timeSpentInMagicStar;
			AverageTimeSpentInTree = dat.timeSpentInTree;
			AverageTimeSpentInDefault = dat.timeSpentInDefault;
			
			
			FilesAdded = dat.filesAdded;
			FilesRemoved = dat.filesRemoved;
			Discoveries = dat.discoveries;

			BeaconsAdded = dat.beaconsAdded;
			BeaconsDestroyed = dat.beaconsDestroyed;
			BeaconsPlanted = dat.beaconsPlanted;

			NestKicks = dat.nestKicks;
			NestSpills = dat.nestSpills;

			logs = dat.logs;
		}
		
		
		#region Analysis

		public float CalculateFocusVolatility()
		{
			float worst = Extensions.StandardDeviation(new float[] {0f, 0f, 0f, 1f});
			float best = Extensions.StandardDeviation(new float[] {.25f, .25f, .25f, .25f});
			
			float[] focuses = new float[] {
				AverageTimeSpentInDefault, 
				AverageTimeSpentInNest,
				AverageTimeSpentInTree,
				AverageTimeSpentInMagicStar
			};

			float deviation = Extensions.StandardDeviation(focuses);
			float volatility = 1f - deviation.RemapNRB(best, worst, 0f, 1f);
			
			return volatility;
		}
		
		#endregion
	}
}