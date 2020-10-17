using UnityEngine;

namespace uwu.Snippets
{
	public class ApplyGravityRelativeToCamera : ApplyCustomGravity
	{
		public float multiplier = 1f;
		UnityEngine.Camera mainCamera;

		public Vector3 gravity
		{
			get
			{
				if (mainCamera == null)
					mainCamera = UnityEngine.Camera.main;

				return -mainCamera.transform.up;
			}
		}

		void Start()
		{
			mainCamera = UnityEngine.Camera.main;
		}

		protected override void ApplyGravity(float magnitude)
		{
			directionOfGravity = gravity;
			base.ApplyGravity(magnitude * multiplier);
		}
	}
}