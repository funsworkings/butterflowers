using UnityEngine;

namespace uwu.Snippets
{
	[ExecuteInEditMode]
	public class VirtualCanvas : MonoBehaviour
	{
		[SerializeField] new UnityEngine.Camera camera;
		[SerializeField] float thickness = 1f;
		float pr_asp = -1f;

		float pr_fov = -1f;
		float pr_ncp = -1f;

		// Update is called once per frame
		void Update()
		{
			if (camera == null) return;

			var ncp = camera.nearClipPlane;
			var fov = camera.fieldOfView;
			var asp = camera.aspect;

			if (ncp != pr_ncp || fov != pr_fov || asp != pr_asp) {
				var depth = ncp + (0.01f + thickness / 2f);

				transform.parent = camera.transform;

				transform.localEulerAngles = Vector3.zero;
				transform.localPosition = new Vector3(0f, 0f, depth);

				var height = Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f) * depth * 2f;
				transform.localScale = new Vector3(height * asp, height, thickness);

				pr_asp = asp;
				pr_fov = fov;
				pr_ncp = ncp;
			}
		}
	}
}