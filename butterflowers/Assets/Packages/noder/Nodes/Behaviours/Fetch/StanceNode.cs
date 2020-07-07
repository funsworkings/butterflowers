using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noder.Nodes.Behaviours.Fetch 
{

    public class StanceNode: Entry<float> {

        protected override float ValueProvider()
        {
            return (graph as ModuleTree).Brain.stance;
        }

    }

}
