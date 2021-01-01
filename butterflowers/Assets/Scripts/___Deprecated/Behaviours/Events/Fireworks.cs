using System;
using System.Collections;
using System.Collections.Generic;
using Neue.Agent;
using Neue.Agent1;
using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;
using Kick = Core.Wand.Kick;
using Wizard;
using Object = UnityEngine.Object;

namespace Noder.Nodes.Behaviours.Events {

    [Obsolete("Obsolete agent!", true)]
    public class Fireworks: BaseEventNode<Object> {

        private Brain brain;

        protected override void OnEnter()
        {
            if (brain == null) {
                var tree = (graph as ModuleTree);
                //brain = tree.Brain;
            }

            base.OnEnter();
        }

        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) {
                //brain.Burst();
                return true;
            }

            return false;
        }
    }

}
