using UnityEngine;

namespace uwu.UI.Extras
{
	[ExecuteInEditMode]
	public class Tooltip : MonoBehaviour
	{
		public new UnityEngine.Camera camera;

		public Transform target;

		[SerializeField] Vector3 offset = Vector3.zero;
		[SerializeField] bool globalOffset = true;

		[SerializeField] bool smooth;
		[SerializeField] float smoothSpeed = 1f;
		UnityEngine.Camera mainCamera;

		bool set;

		bool m_active = false;

		public bool active
		{
			get { return m_active;  }
			set
			{
				m_active = value;
			}
		}

		void Awake()
		{
			mainCamera = UnityEngine.Camera.main;
		}

		void Update()
		{
			if (active) {
				if (target == null) {
					set = false;
					return;
				}

				var pos = CalculatePosition();
				if (set && smooth) {
					transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * smoothSpeed);
					return;
				}

				transform.position = pos;
				set = true;
			}
		}

		Vector3 CalculatePosition()
		{
			var cam = camera == null ? mainCamera : camera;

			if (globalOffset)
				return cam.WorldToScreenPoint(target.position) + offset;
			return cam.WorldToScreenPoint(target.position + target.TransformVector(offset));
		}
	}
}