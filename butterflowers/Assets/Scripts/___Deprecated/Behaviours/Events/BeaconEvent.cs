using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Behaviours.Core;
using Noder.Graphs;
using System.Linq;

namespace Noder.Nodes.Behaviours.Events {

    [Obsolete("Obsolete API!", true)]
    public class BeaconEvent: BaseEventNode<string> {

        public enum Type { Add, Remove, Plant }

        [Input(ShowBackingValue.Always, ConnectionType.Override)] public Type type = Type.Add;

        protected override bool FireEvent()
        {
            var tree = (graph as ModuleTree);
            if (tree != null) 
            {
                var beacon = GetInputValue<string>("data", this.data);

                if (type == Type.Add) {
                    if (beacon != null) {
                        ModuleTree.onReceiveEvent(tree, EVENTCODE.BEACONACTIVATE, beacon, rewards);
                        return true;
                    }
                }
                else if (type == Type.Remove) {
                    ModuleTree.onReceiveEvent(tree, EVENTCODE.NESTPOP, beacon, rewards);
                    return true;
                }
                else if (type == Type.Plant) {
                    ModuleTree.onReceiveEvent(tree, EVENTCODE.BEACONPLANT, beacon, rewards);
                    return true;
                }
            }

            return false;
        }
    }

}
