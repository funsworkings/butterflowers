using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;
using Kick = Core.Wand.Kick;

namespace Noder.Nodes.Behaviours.Events {

    [Obsolete("Obsolete API!", true)]
    public class KickNest: BaseEventNode<Vector3> {

        private Nest nest;

        public bool useDirection = false;
        public float force = 1f;

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
                    kick.force = force;


                ModuleTree.onReceiveEvent(tree, EVENTCODE.NESTKICK, kick, rewards); // Override event
                return true;
            }

            return false;
        }
    }

}
