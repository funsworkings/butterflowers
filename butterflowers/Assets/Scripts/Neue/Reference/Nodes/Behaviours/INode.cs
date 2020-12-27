using UnityEngine;

namespace Neue.Reference.Nodes.Behaviours
{
	public interface INode
	{
		string Name { get; }
		
		GameObject Object { get; }
		Collider Collider { get; }
	}
}