using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract
{
    [NodeTint(.87f, .87f, .87f)]
    public abstract class Listener<E> : Node
    {
        [Input(ShowBackingValue.Never, typeConstraint = TypeConstraint.Inherited)] public Action actions;

        public virtual void Receive(E dat) { Next(); }

        public override void Next()
        {
            Action[] actionables = GetInputValues<Action>("actions");
            for (int i = 0; i < actionables.Length; i++) {
                if (actionables[i] != null)
                    actionables[i].Enact();
            }
        }
    }
}

