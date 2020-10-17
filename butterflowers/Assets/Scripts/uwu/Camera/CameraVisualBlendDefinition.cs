using UnityEngine;

namespace uwu.Camera
{
	[CreateAssetMenu(fileName = "New Camera Visual Blend Def", menuName = "Internal/Camera/Blend", order = 52)]
	public class CameraVisualBlendDefinition : ScriptableObject
	{
		public enum Type
		{
			Opacity,
			Animation
		}

		public Type type = Type.Opacity;
		public AnimationClip animation;
	}
}