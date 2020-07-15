using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract {

    public abstract class Flexible<E> : Entry<E> {

        [Input(ShowBackingValue.Never, ConnectionType.Multiple)] public Node input;
        [Output(ShowBackingValue.Never, ConnectionType.Override)] public Node success, fail;

        protected E val = default(E);


        public override void Next()
        {
            NodePort port = null; ;

            try {
                val = ValueProvider();
                port = GetOutputPort("success");
            }
            catch (System.Exception e) 
            {
                //Debug.LogException(e);
                port = GetOutputPort("fail");
            }

            if (port != null) {
                SendSignalToOutputs(new NodePort[] { port });
                Exit(); // Exit when moving to next
            }
        }

        protected override void OnEnter()
        {
            Graph.activeNode = (this as Node);
            if (Graph.auto)
                Next();
        }

        protected override void OnExit() { }

    }

}
