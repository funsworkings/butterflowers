using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using Neue.Reference.Types.Maps;
using UnityEngine;

namespace Noder.Nodes.Behaviours.States {

    public class ModuleNode: State {

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public ModuleTree tree;
        
        public FrameInt rewards;

        protected override void OnEnter()
        {
            base.OnEnter();

            var val = GetInputValue<ModuleTree>("tree", this.tree);

            if (val != null) 
            {
                Debug.Log("MOVE TO " + val.name);

                var t = (graph as ModuleTree);
                if (t != null) 
                {
                    ModuleTree.onReceiveRewards(t, rewards);
                }
                
                val.Restart(); // Move into sub-tree
                t.Reset(); // Reset current tree
            }
                
        }

    }

}
