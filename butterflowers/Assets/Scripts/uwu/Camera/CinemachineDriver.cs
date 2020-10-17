using Cinemachine;
using UnityEngine;

namespace uwu.Camera
{
	public class CinemachineDriver : MonoBehaviour
	{
		new CinemachineVirtualCamera camera;
		CinemachineComposer composer;

		bool fetched;

		CinemachineTransposer transposer;

		void Awake()
		{
			Fetch();
		}

		void Fetch()
		{
			camera = GetComponent<CinemachineVirtualCamera>();
			transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
			composer = camera.GetCinemachineComponent<CinemachineComposer>();

			fetched = true;
		}

		public void SetNearClipping(float near)
		{
			if (!fetched) Fetch();
			camera.m_Lens.NearClipPlane = near;
		}

		public void SetFarClipping(float far)
		{
			if (!fetched) Fetch();
			camera.m_Lens.FarClipPlane = far;
		}

		public void UpdateTransposerOffset(Vector3 offset)
		{
			if (!fetched) Fetch();
			transposer.m_FollowOffset = offset;
		}

		public void UpdateComposerOffset(Vector3 offset)
		{
			if (!fetched) Fetch();
			composer.m_TrackedObjectOffset = offset;
		}
	}
}