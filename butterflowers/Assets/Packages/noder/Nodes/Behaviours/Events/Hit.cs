using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;
using Wizard;

namespace Noder.Nodes.Behaviours.Events {

    public class Hit: BaseEventNode<Object> {

        private Controller controller;

        protected override void OnEnter()
        {
            if (controller == null) 
            {
                var tree = (graph as ModuleTree);
                controller = tree.Brain.controller;
            }

            base.OnEnter();
        }

        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) 
            {
                controller.Hit();
                return true;
            }

            return false;
        }
    }

}
