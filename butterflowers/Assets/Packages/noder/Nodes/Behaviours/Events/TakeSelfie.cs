using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Events {

    public class TakeSelfie: BaseEventNode<string> 
    {
        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) {
                string name = GetInputValue<string>("data", this.data);

                ModuleTree.onReceiveEvent(tree, EVENTCODE.PHOTOGRAPH, name); // Override event
                return true;
            }

            return false;
        }
    }

}
