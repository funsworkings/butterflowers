using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;
using Kick = Wand.Kick;

namespace Noder.Nodes.Behaviours.Events {

    public class KickNest: BaseEventNode<Vector3> {

        private Nest nest;

        public bool useDirection = false;

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
                Vector3 dir = GetInputValue<Vector3>("data", this.data);

                Kick kick = new Kick();
                kick.useDirection = useDirection;
                kick.direction = dir;


                ModuleTree.onReceiveEvent(tree, EVENTCODE.NESTKICK, kick); // Override event
                return true;
            }

            return false;
        }
    }

}
