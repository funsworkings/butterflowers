using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.Camera
{
	/// <summary>
	///     Base class for game cameras in scene
	/// </summary>
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	public class GameCamera : MonoBehaviour
	{
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
			if (camera != null) {
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
		}

		protected virtual void onDisabled()
		{
		}

		#endregion
	}
}