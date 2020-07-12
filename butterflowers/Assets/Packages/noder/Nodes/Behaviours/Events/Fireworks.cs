using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;
using Kick = Wand.Kick;
using Wizard;

namespace Noder.Nodes.Behaviours.Events {

    public class Fireworks: BaseEventNode<Object> {

        private Brain brain;

        protected override void OnEnter()
        {
            if (brain == null) {
                var tree = (graph as ModuleTree);
                brain = tree.Brain;
            }

            base.OnEnter();
        }

        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) {
                brain.Burst();
                return true;
            }

            return false;
        }
    }

}
