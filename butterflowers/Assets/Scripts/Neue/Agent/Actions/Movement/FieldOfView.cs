using Neue.Agent.Presets;
using Neue.Agent.Types;
using UnityEngine;
using uwu.Animation;

namespace Neue.Agent.Actions.Movement
{
	public class FieldOfView : Module
	{
		// Properties

		[SerializeField] MovementPreset preset = null;
		
		IK ik;
		Navigation navigation;

		// Attributes

		[SerializeField] Transform focalPoint = null;
		
		#region Accessors
		
		public Transform FocalPoint { get => focalPoint; set { focalPoint = value; } }
		
		#endregion

		void Awake()
		{
			ik = GetComponent<IK>();
			navigation = GetComponent<Navigation>();
		}

		#region Falloff

		float CalculateAngleToFocalPoint()
		{
			Vector3 forwards = ik.heading;
			Vector3 direction = (focalPoint.position - ik.head.position).normalized;

			return Vector3.SignedAngle(forwards, direction, transform.up);
		}
		
		#endregion

		#region Module

		public override void Continue()
		{
			ik.LookAtTarget = focalPoint;

			bool tracking = (focalPoint != null);
			if (tracking) 
			{
				float angleBetween = CalculateAngleToFocalPoint();
				float angleMagnitude = Mathf.Clamp01(Mathf.Abs(angleBetween) / preset.fovFalloffAngle);
				float lookAtStrength = 1f - preset.fovFalloffCurve.Evaluate(angleMagnitude);

				ik.lookAtWeight = lookAtStrength;
			}
			else 
			{
				ik.lookAtWeight = 0f;
			}
		}

		public override void Pause()
		{
			throw new System.NotImplementedException();
		}

		public override void Destroy()
		{
			throw new System.NotImplementedException();
		}
		
		#endregion
	}
}