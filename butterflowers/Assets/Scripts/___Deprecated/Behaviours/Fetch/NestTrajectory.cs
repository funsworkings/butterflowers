using System;
using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

namespace Noder.Nodes.Behaviours.Fetch 
{

    [Obsolete("Obsolete API!", true)]
    public class NestTrajectory: Entry<Vector3> {

        protected override Vector3 ValueProvider()
        {
            return (graph as ModuleTree).Brain.Nest.trajectory;
        }

    }

}
