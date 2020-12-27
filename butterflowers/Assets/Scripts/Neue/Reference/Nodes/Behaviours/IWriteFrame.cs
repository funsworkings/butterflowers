using UnityEngine;

namespace Neue.Reference.Nodes.Behaviours
{
	public interface IWriteFrame : INode
	{
		/// <summary>
		/// Applies a frame offset to node
		/// </summary>
		/// <param name="value">Offset</param>
		/// <returns>Resultant value of operation</returns>
		Vector4 WriteFrame(Vector4 value);
	}
}