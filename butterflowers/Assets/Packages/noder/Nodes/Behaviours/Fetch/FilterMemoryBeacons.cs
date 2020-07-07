using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Entries {

    public class FilterMemoryBeacons: Entry<string[]> {

        public enum Filter {
            Negative = -1,
            Neutral = 0,
            Positive = 1
        }
        public Filter filter = Filter.Neutral;

        [Input(ShowBackingValue.Unconnected, ConnectionType.Override)] public string[] beacons;

        protected override string[] ValueProvider()
        {
            var brain = (graph as ModuleTree).Brain;
            var raw = GetInputValue<string[]>("beacons", this.beacons);

            return brain.filterMemoryBeacons(raw, (int)filter);
        }

    }

}
