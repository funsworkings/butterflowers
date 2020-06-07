using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract {

    public abstract class State : Node
    {
        [Input(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Inherited)] public Node input;
        [Output(ShowBackingValue.Never, ConnectionType.Override)] public Node output;

        public override void Next()
        {
            NodePort port = GetOutputPort("output");
            if (port != null) {
                SendSignalToOutputs(new NodePort[] { port });
                Exit(); // Exit when moving to next
            }
        }

        protected override void OnEnter()
        {
            Graph.activeNode = (this as Node);
        }

        protected override void OnExit() { }
    }

}
