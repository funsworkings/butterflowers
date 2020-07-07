using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;
using Kick = Wand.Kick;

namespace Noder.Nodes.Behaviours.Events {

    public class ClearNest: BaseEventNode<Object> {

        private Nest nest;

        protected override void OnEnter()
        {
            if (nest == null) {
                var tree = (graph as ModuleTree);
                nest = tree.Brain.Nest;
            }

            base.OnEnter();
        }

        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) {
                ModuleTree.onReceiveEvent(tree, EVENTCODE.NESTCLEAR, null); // Override event
                return true;
            }

            return false;
        }
    }

}
