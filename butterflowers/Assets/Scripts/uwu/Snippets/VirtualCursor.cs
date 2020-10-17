using UnityEngine;

namespace uwu.Snippets
{
	public class VirtualCursor : Cursor
	{
		[SerializeField] new UnityEngine.Camera camera;

		protected override Vector3 Position()
		{
			return camera.WorldToScreenPoint(transform.position);
		}
	}
}