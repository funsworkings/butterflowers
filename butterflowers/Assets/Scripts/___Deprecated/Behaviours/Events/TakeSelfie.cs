using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Events {

    [Obsolete("Obsolete API!", true)]
    public class TakeSelfie: BaseEventNode<string> 
    {
        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) {
                string name = GetInputValue<string>("data", this.data);

                ModuleTree.onReceiveEvent(tree, EVENTCODE.PHOTOGRAPH, name, rewards); // Override event
                return true;
            }

            return false;
        }
    }

}
