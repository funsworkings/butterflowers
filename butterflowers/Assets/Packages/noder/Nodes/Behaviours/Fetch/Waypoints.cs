﻿using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noder.Nodes.Behaviours.Fetch {

    public class Waypoints: Entry<Focusable[]> {

        protected override Focusable[] ValueProvider()
        {
            var brain = (graph as ModuleTree).Brain;
            return brain.getWaypoints();
        }

    }

}
