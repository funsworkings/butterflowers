using UnityEngine;
using uwu.Extensions;

namespace uwu.Camera
{
	public class CameraDriver : Singleton<CameraDriver>
	{
		UnityEngine.Camera m_camera;

		public new UnityEngine.Camera camera
		{
			get
			{
				if (m_camera == null)
					m_camera = UnityEngine.Camera.main;

				return m_camera;
			}
		}

		public Vector3 MoveRelativeToCamera(Vector2 vector)
		{
			return MoveRelativeToCamera(new Vector3(vector.x, vector.y, 0f));
		}

		public Vector3 MoveRelativeToCamera(Vector3 vector)
		{
			vector.z = 0f;
			return camera.transform.TransformVector(vector);
		}

		public Vector3 ConvertToScreen(Vector3 position)
		{
			return camera.WorldToScreenPoint(position);
		}

		public Vector3 ConvertToViewport(Vector3 position)
		{
			return camera.WorldToViewportPoint(position);
		}
	}
}