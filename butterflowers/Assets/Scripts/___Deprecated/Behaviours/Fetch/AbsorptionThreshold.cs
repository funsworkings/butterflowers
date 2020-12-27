using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Fetch {

    [Obsolete("Obsolete API!", true)]
    public class AbsorptionThreshold: Entry<float> {

        protected override float ValueProvider()
        {
            var tree = (graph as ModuleTree);
            return tree.Brain.OldPreset.absorptionThreshold;
        }
    }

}
