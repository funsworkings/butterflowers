using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS;
using butterflowersOS.Objects.Entities.Interactables;
using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;
using Kick = butterflowersOS.Core.Wand.Kick;
using Object = UnityEngine.Object;

namespace Noder.Nodes.Behaviours.Events {

    [Obsolete("Obsolete API!", true)]
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
                ModuleTree.onReceiveEvent(tree, EVENTCODE.NESTCLEAR, null, rewards); // Override event
                return true;
            }

            return false;
        }
    }

}
