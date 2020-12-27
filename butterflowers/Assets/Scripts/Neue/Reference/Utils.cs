using Neue.Reference.Types;
using UnityEngine;

namespace Neue.Reference
{
	public static class Utils
	{
		public static float FrameValue(this Vector4 vector, Frame frame)
		{
			int frameIndex = (int) frame;
			if(frameIndex < 0 || frameIndex > 3) throw new System.Exception("Expected frame outside of vector4 bounds!");
			
			if (frameIndex == 0) return vector.x;
			if (frameIndex == 1) return vector.y;
			if (frameIndex == 2) return vector.z;
			return vector.w;
		}

		public static Vector4 VectorValue(this float value, Frame frame)
		{
			int frameIndex = (int) frame;
			if(frameIndex < 0 || frameIndex > 3) throw new System.Exception("Expected frame outside of vector4 bounds!");
			
			Vector4 vector = Vector4.zero;

			if (frameIndex == 0) vector.x = value;
			else if (frameIndex == 1) vector.y = value;
			else if (frameIndex == 2) vector.z = value;
			else vector.w = value;

			return vector;
		}
	}
}