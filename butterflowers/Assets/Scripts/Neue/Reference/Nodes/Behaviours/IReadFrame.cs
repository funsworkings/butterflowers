using UnityEngine;

namespace Neue.Reference.Nodes.Behaviours
{
	public interface IReadFrame : INode
	{
		/// <summary>
		/// Returns the current frame value for node
		/// </summary>
		/// <returns>Frame value</returns>
		Vector4 GetFrame();
	}
}