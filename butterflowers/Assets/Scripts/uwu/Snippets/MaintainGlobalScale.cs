using UnityEngine;

namespace uwu.Snippets
{
	[ExecuteInEditMode]
	public class MaintainGlobalScale : MonoBehaviour
	{
		[SerializeField] Vector3 scale;

		// Update is called once per frame
		void Update()
		{
			var globalscale = transform.lossyScale;

			var dx = scale.x / globalscale.x;
			var dy = scale.y / globalscale.y;
			var dz = scale.z / globalscale.z;

			transform.localScale = Vector3.Scale(transform.localScale, new Vector3(dx, dy, dz));
		}
	}
}