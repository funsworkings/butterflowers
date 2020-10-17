using UnityEngine;
using uwu.Camera;

namespace uwu.Snippets
{
	public class Billboard : MonoBehaviour
	{
		public bool refresh;

		[SerializeField] bool continuous = true;
		CameraManager CameraManager;

		// Start is called before the first frame update
		void Start()
		{
			CameraManager = FindObjectOfType<CameraManager>();
		}

		// Update is called once per frame
		void Update()
		{
			if (refresh || continuous) {
				var camera = CameraManager.MainCamera;
				transform.forward = (camera.transform.position - transform.position).normalized;

				refresh = false;
			}
		}
	}
}