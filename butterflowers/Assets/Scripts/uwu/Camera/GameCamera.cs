using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using uwu.Extensions;

namespace uwu.Camera
{
	/// <summary>
	///     Base class for game cameras in scene
	/// </summary>
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	public class GameCamera : MonoBehaviour
	{
		public static GameCamera ActiveCamera = null;
		
		
		#region Events

		public UnityEvent onEnable, onDisable;

		#endregion

		// Connect to Cinemachine camera
		protected new CinemachineVirtualCamera camera;

		protected CinemachineFreeLook cameraFreeLook;
		protected CinemachineComposer composer;

		protected CameraManager manager;
		protected CinemachineTransposer transposer;

		#region Accessors

		public float fov => camera.m_Lens.FieldOfView;
		public float nearClipPlane => camera.m_Lens.NearClipPlane;
		public float farClipPlane => camera.m_Lens.FarClipPlane;

		public bool IsActive => (ActiveCamera == this);

		#endregion

		#region Monobehaviour callbacks

		protected virtual void Awake()
		{
			manager = FindObjectOfType<CameraManager>();

			camera = GetComponent<CinemachineVirtualCamera>();
			cameraFreeLook = GetComponent<CinemachineFreeLook>();
		}

		protected virtual void Start()
		{
			if (camera != null) 
			{
				transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
				composer = camera.GetCinemachineComponent<CinemachineComposer>();
			}
		}

		public virtual void Enable()
		{
			Debug.Log("Enabled " + name);
			camera.enabled = true;

			onEnabled();
			onEnable.Invoke();

			ActiveCamera = this;
		}

		protected virtual void onEnabled()
		{
		}

		public virtual void Disable()
		{
			Debug.Log("Disabled " + name);
			camera.enabled = false;

			onDisabled();
			onDisable.Invoke();

			if (ActiveCamera == this) ActiveCamera = null;
		}

		protected virtual void onDisabled()
		{
		}

		#endregion
		
		#region Alignment
		
		// 0 = Perpendicular 1 = Same direction -1 = Opposite direction
		public float CalculateAlignmentWithTarget(Transform target)
		{
			if (target == transform) return 1f; // Trajectory of self

			Vector3 dirBetween = (target.position - transform.position).normalized;
			Vector3 cameraForward = transform.forward;

			float dot = Vector3.Dot(dirBetween, cameraForward);
			return dot.RemapNRB(-1f, 1f, 0f, 1f);
		}
		
		#endregion
	}
}