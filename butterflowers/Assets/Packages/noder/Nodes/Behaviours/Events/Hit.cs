using System.Collections;
using System.Collections.Generic;
using Neue.Agent;
using Neue.Agent1;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;
using Wizard;

namespace Noder.Nodes.Behaviours.Events {

    public class Hit: BaseEventNode<Object> {

        private Actions actions;

        protected override void OnEnter()
        {
            if (actions == null) 
            {
                var tree = (graph as ModuleTree);
                actions = tree.Brain.Actions;
            }

            base.OnEnter();
        }

        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) 
            {
                actions.Hit();
                return true;
            }

            return false;
        }
    }

}
