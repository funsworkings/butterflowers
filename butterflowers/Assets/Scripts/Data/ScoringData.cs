using UnityEngine;

namespace Data
{
	[System.Serializable]
	public class ScoringData : SurveillanceData
	{
		public float AverageHoB = 0f;
		public float AverageNestFill = 0f;
		public float AverageCursorSpeed = 0f;

		public float AverageTimeSpentInNest = 0f;
		public float AverageTimeSpentInMagicStar = 0f;
		public float AverageTimeSpentInTree = 0f;
		public float AverageTimeSpentInDefault = 0f;

		public ScoringData(){}
		
		public ScoringData(SurveillanceData dat)
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
	}
}