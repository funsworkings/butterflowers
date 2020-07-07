using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract {

    public abstract class Branch<E>: State {

        [Input(ShowBackingValue.Unconnected, ConnectionType.Multiple, TypeConstraint.Strict)] public E value;
        [Output(dynamicPortList = true)] public E[] routes;

        public override void Next()
        {
            E val = GetInputValue<E>("value", this.value);
            Next(val);
        }

        public void Next(E val)
        {
            if (!routes.Contains(val)) {
                base.Next();
                return;
            }

            for (int i = 0; i < routes.Length; i++) {
                if (routes[i].Equals(val)) {
                    Route(i);
                    return;
                }
            }

            base.Next();
        }

        public void Index(int index)
        {
            bool flag = (routes.Length == 0 || routes.Length <= index);
            if (flag) {
                base.Next();
                return;
            }

            Route(index);
        }

        protected void Route(int index)
        {
            NodePort port = GetOutputPort("routes " + index);
            if (port == null) 
            {
                base.Next();
                return;
            }

            SendSignalToOutputs(new NodePort[] { port });
        }
    }

}
