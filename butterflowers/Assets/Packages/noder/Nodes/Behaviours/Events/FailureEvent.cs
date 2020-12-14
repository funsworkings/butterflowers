using Neue.Agent.Actions.Types;
using Neue.Types;
using Noder.Graphs;
using Noder.Nodes.Behaviours.Core;

namespace Noder.Nodes.Behaviours.Events
{
	public class FailureEvent : BaseEventNode<FailureCode>
	{
		protected override bool FireEvent()
		{
			var tree = (graph as ModuleTree);
			if (tree != null) {
				var error = GetInputValue<FailureCode>("data", this.data);

				ModuleTree.onFailEvent(tree, error);
				return true;
			}

			return false;
		}
	}
}