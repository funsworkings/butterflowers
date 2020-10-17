using UnityEngine;

namespace uwu.Snippets
{
	[RequireComponent(typeof(RectTransform))]
	public class FollowCursor : MonoBehaviour
	{
		[Range(0f, 1f)] public float lerpAmount = 1f;

		// Start is called before the first frame update
		void Start()
		{
			transform.position = Input.mousePosition;
		}

		// Update is called once per frame
		void Update()
		{
			Track();
		}

		void Track()
		{
			var current = transform.position;
			var target = Input.mousePosition;

			var dir = target - current;

			transform.position = current + dir * lerpAmount;
		}
	}
}