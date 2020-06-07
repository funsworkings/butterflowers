using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;

namespace Noder.Nodes.States
{
    public class Event : SimpleState
    {
        [Input(ShowBackingValue.Never, typeConstraint = TypeConstraint.Inherited)] public Action actions;

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

            base.Next();
        }
    }
}


