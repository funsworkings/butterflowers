using UnityEngine;

namespace uwu.Snippets
{
	public class Follow : MonoBehaviour
	{
		[SerializeField] Transform target = null;
		[SerializeField] Vector3 offset = Vector3.zero;

		[SerializeField] bool x = true, y = true, z = true;

		// Update is called once per frame
		void Update()
		{
			if (target == null || !x && !y && !z) return;

			var pos = transform.position;

			if (x) pos.x = target.position.x + offset.x;
			if (y) pos.y = target.position.y + offset.y;
			if (z) pos.z = target.position.z + offset.z;

			transform.position = pos;
		}
	}
}