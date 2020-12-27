using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.Behaviours.Fetch {

    [Obsolete("Obsolete API!", true)]
    public class LastEvent: Entry<EVENTCODE> {

        public enum Type { PLAYER, WORLD, NEST }
        public Type type = Type.PLAYER;

        protected override EVENTCODE ValueProvider()
        {
            var brain = (graph as ModuleTree).Brain;

            if (type == Type.WORLD) 
                return brain.LAST_WORLD_EVENT;
            else if (type == Type.NEST)
                return brain.LAST_NEST_EVENT;

            return brain.LAST_PLAYER_EVENT;
        }
    }

}
