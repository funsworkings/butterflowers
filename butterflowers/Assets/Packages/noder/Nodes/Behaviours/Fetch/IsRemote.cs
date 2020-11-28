using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noder.Nodes.Behaviours.Fetch 
{

    public class IsRemote: Entry<bool> {

        protected override bool ValueProvider()
        {
            return (graph as ModuleTree).Brain.Remote;
        }

    }

}
