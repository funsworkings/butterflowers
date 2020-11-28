using System.Linq;
using UnityEngine;
using uwu.Extensions;

namespace Data
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

		public CompositeSurveillanceData(){}
		
		public CompositeSurveillanceData(SurveillanceData dat)
		{
			this.filesAdded = dat.filesAdded;
			this.filesRemoved = dat.filesRemoved;
			this.discoveries = dat.discoveries;

			this.beaconsAdded = dat.beaconsAdded;
			this.beaconsPlanted = dat.beaconsPlanted;

			this.nestKicks = dat.nestKicks;
			this.nestSpills = dat.nestSpills;

			this.logs = dat.logs;
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