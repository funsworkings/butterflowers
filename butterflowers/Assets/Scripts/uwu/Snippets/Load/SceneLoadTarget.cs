using UnityEngine;

namespace uwu.Snippets.Load
{
	[System.Serializable]
	public class SceneLoadTarget
	{
		public int from;
		public int to;

		[Range(0f, 1f)] public float duration = .5f;
	}
}