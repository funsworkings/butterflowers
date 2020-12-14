using UnityEngine;
using uwu.Extensions;

namespace Neue.Agent.Presets
{
	[CreateAssetMenu(fileName = "New Movement Preset", menuName = "Presets/Neue Agent/Movement", order = 0)]
	public class MovementPreset : ScriptableObject
	{
		public float moveSpeed = 1f;
		public AnimationCurve moveSpeedCurve;
		public float acceleration = 1f;
		
		public float minTurnSpeed = 1f, maxTurnSpeed = 5f;
		public AnimationCurve turnSpeedCurve;
		
		#region Accessors

		public float GetTurnSpeedFromBearing(float angle)
		{
			float ang_magnitude = angle.RemapNRB(0f, 180f, 0f, 1f);
			float magnitude = turnSpeedCurve.Evaluate(ang_magnitude);

			float speed = magnitude * maxTurnSpeed;
			return speed;
		}

		public float GetMoveSpeedFromBearing(float angle)
		{
			float ang_magnitude = angle.RemapNRB(0f, 180f, 0f, 1f);
			float magnitude = moveSpeedCurve.Evaluate(ang_magnitude);

			return (1f - magnitude) * moveSpeed;
		}
		
		#endregion
	}
}