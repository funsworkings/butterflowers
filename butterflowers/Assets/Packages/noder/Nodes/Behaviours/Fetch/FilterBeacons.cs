using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Entries {

    public class FilterBeacons: Entry<string[]> {

        public enum Filter {
            None,
            Active,
            Inactive,
            Unknown,
            Comfortable,
            Memory,

            Playable,
            Actionable
        }
        public Filter filter = Filter.None;

        [Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict)] public string[] beacons;

        protected override string[] ValueProvider()
        {
            var brain = (graph as ModuleTree).Brain;
            var raw = GetInputValue<string[]>("beacons", this.beacons);

            return brain.filterBeacons(raw, filter);
        }

    }

}
