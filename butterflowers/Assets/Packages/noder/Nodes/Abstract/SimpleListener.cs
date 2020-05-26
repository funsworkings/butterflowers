using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract
{
    [NodeTint(.87f, .87f, .87f)]
    public abstract class SimpleListener<T> : Listener<T>
    {
        [Input(ShowBackingValue.Never, typeConstraint = TypeConstraint.Inherited)] public Action actions;

        public override void Receive(T dat){ Next(); }

        public override void Next(){
            Action[] actionables = GetInputValues<Action>("actions");
            for(int i = 0; i < actionables.Length; i++){
                if(actionables[i] != null) 
                    actionables[i].Enact();
            }
        }
    }
}

