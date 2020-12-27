using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Fetch {

    [Obsolete("Obsolete API!", true)]
    public class Absorption: Entry<float> {

        protected override float ValueProvider()
        {
            return 0f;
            //return (graph as ModuleTree).Brain.absorption;
        }

    }

}
