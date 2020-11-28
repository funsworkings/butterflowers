using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Events {

    public class EnactGesture: BaseEventNode<Gesture> 
    {
        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) {
                Gesture dat = GetInputValue<Gesture>("data", this.data);

                ModuleTree.onReceiveEvent(tree, EVENTCODE.GESTURE, dat, rewards); // Override event
                return true;
            }

            return false;
        }
    }

}
