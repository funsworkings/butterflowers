using Cinemachine;
using UnityEngine;

namespace uwu.Camera.Instances
{
	public class RevolveCamera : GameCamera
	{
		[SerializeField] Transform target;
		[SerializeField] bool controls;

		[Header("Attributes")] [SerializeField]
		float radius = 1f, height = 0f;

		[SerializeField] float angle = 0f;
		[SerializeField] bool resetOnDisable = false;

		float defaultAngle;

		CinemachineOrbitalTransposer orbitTransposer;

		protected override void Start()
		{
			base.Start();

			orbitTransposer = camera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
			defaultAngle = angle;
		}

		// Update is called once per frame
		void Update()
		{
			if (orbitTransposer == null) return;

			var angToRad = angle * Mathf.Deg2Rad;
			var offset = new Vector3(Mathf.Cos(angToRad) * radius, height, Mathf.Sin(angToRad) * radius);

			orbitTransposer.m_FollowOffset = offset;
		}

		protected override void onDisabled()
		{
			base.onDisabled();

			if (resetOnDisable) {
				//angle = defaultAngle;
			}
		}
	}
}