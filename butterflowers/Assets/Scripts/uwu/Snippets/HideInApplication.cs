using UnityEngine;

namespace uwu.Snippets
{
	public class HideInApplication : MonoBehaviour
	{
		Renderer[] renderers;

		void Awake()
		{
			renderers = GetComponentsInChildren<Renderer>();
		}

		// Use this for initialization
		void Start()
		{
			foreach (var r in renderers)
				r.enabled = false;
		}
	}
}