using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.States
{
    public class Basic : SimpleState
    {
        [Input(ShowBackingValue.Never, typeConstraint = TypeConstraint.Inherited)] public Action actions;
        [Output] public Node output;

        protected string description = "Basic";
        public string Description {
            get{
                return description;
            }
            set{
                description = value;
            }
        }

        public override void Next(){
            Action[] actionables = GetInputValues<Action>("actions");
            for(int i = 0; i < actionables.Length; i++){
                if(actionables[i] != null) 
                    actionables[i].Enact();
            }

            XNode.NodePort port = GetOutputPort("output");
                SendSignalToOutputs(new XNode.NodePort[]{ port });

            Exit();
        }

        protected override void OnExit(){}
    }
}


