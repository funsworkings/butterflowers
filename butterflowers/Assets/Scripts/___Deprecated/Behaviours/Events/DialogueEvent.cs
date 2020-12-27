using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Events {

    [Obsolete("Obsolete API!", true)]
    public class DialogueEvent: BaseEventNode<string> {

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public float delay = 0f;

        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) {
                string val = GetInputValue<string>("data", this.data);
                float del = GetInputValue<float>("delay", this.delay);

                if (!string.IsNullOrEmpty(val)) 
                {
                    ModuleTree.onReceiveDialogue(tree, val, del);
                    return true;
                }
            }

            return false;
        }

    }

}
