using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noder.Nodes.Behaviours.States {

    public class ModuleNode: State {

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public ModuleTree tree;

        protected override void OnEnter()
        {
            base.OnEnter();

            var val = GetInputValue<ModuleTree>("tree", this.tree);

            if (val != null) 
            {
                val.Restart(); // Move into sub-tree
                (graph as ModuleTree).Reset(); // Reset current tree
            }
                
        }

    }

}
