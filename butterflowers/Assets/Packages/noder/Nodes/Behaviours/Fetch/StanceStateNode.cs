using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

namespace Noder.Nodes.Behaviours.Fetch 
{

    public class StanceStateNode: Entry<Brain.StanceState> {

        public bool environment = false;

        protected override Brain.StanceState ValueProvider()
        {
            return (graph as ModuleTree).Brain.getStanceState((environment) ? AGENT.World : AGENT.NULL);
        }

    }

}
