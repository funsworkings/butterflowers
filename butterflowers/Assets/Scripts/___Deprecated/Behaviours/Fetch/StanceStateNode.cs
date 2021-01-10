using System;
using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS;
using Neue.Agent.Brain.Types;
using Neue.Types;
using UnityEngine;
using Wizard;

namespace Noder.Nodes.Behaviours.Fetch 
{

    [Obsolete("Obsolete API!", true)]
    public class StanceStateNode: Entry<Stance> {

        public bool environment = false;

        protected override Stance ValueProvider()
        {
            return (graph as ModuleTree).Brain.getStanceState((environment) ? AGENT.World : AGENT.NULL);
        }

    }

}
