using UnityEngine;

namespace uwu.Snippets
{
	[ExecuteInEditMode]
	public class ReverseLocalEulerAngles : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{
		}

		// Update is called once per frame
		void Update()
		{
			var parent = transform.parent;

			if (parent == null)
				return;

			transform.localEulerAngles = -parent.localEulerAngles;
		}
	}
}