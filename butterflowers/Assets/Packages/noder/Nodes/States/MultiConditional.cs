using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;
using System.Linq;
using Noder.Nodes.Core.Operations;

namespace Noder.Nodes.States
{
    public class MultiConditional : SimpleState
    {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)] public float a;
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)] public Condition condition;

        [Output(dynamicPortList = true)] public float[] b;


        public override void Next(){
            float val = GetInputValue<float>("a", a);
            Condition con = GetInputValue<Condition>("condition", condition);

            int ind = 0;
            for (; ind < b.Length; ind++) {
                if (con.Satisfies(a, b[ind]))
                    break;
            }

            if (ind < b.Length) {
                NodePort port = GetOutputPort(string.Format("routes {0}", ind));
                SendSignalToOutputs(new NodePort[] { port });
            }
            Exit();
         }

        protected override void OnExit(){}
    }
}


