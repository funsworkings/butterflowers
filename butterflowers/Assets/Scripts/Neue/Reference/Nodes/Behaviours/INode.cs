using Neue.Reference.Types;

namespace Neue.Reference.Nodes.Behaviours
{
	public interface INode
	{
		/// <summary>
		/// Calculate the approximation in correlation for IResponder
		/// </summary>
		/// <param name="ref">Target frame of reference</param>
		/// <returns>Proximity from 0-1</returns>
		float GetValueForReference(Frame @ref);
		
		/// <summary>
		/// Respond to a push towards a specific frame of reference
		/// </summary>
		/// <param name="ref">Target frame of reference</param>
		/// <param name="value">The value from 0-1 of push to reference</param>
		/// <returns>Success condition of the operation</returns>
		bool SetValueForReference(Frame @ref, float value);
	}
}