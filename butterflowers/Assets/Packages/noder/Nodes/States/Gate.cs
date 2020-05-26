using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.States
{
    public class Gate : SimpleState
    {
        [Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict)] public bool value;
        [Output] public Node pass, fail;


        public override void Next(){
            bool passes = GetInputValue<bool>("value", value);

            NodePort port;
            if(passes) // passes!
                port = GetOutputPort("pass");
            else  // fails!
                port = GetOutputPort("fail");
 
            SendSignalToOutputs(new NodePort[]{ port });
            Exit();
         }

        protected override void OnExit(){}
    }
}


