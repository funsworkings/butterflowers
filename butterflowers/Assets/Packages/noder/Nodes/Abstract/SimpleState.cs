using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;

namespace Noder.Nodes.Abstract
{
    public abstract class SimpleState : State
    {
        protected override void OnEnter(){ 
            Graph.activeNode = (this as Node);
        }
    }
}


