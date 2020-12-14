using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using Neue.Agent.Brain.Types;
using Neue.Types;
using UnityEngine;
using Wizard;

namespace Noder.Nodes.Behaviours.Fetch 
{

    public class StanceStateNode: Entry<Stance> {

        public bool environment = false;

        protected override Stance ValueProvider()
        {
            return (graph as ModuleTree).Brain.getStanceState((environment) ? AGENT.World : AGENT.NULL);
        }

    }

}
