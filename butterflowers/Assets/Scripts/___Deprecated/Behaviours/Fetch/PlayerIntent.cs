using System;
using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS;
using UnityEngine;


namespace Noder.Nodes.Behaviours.Fetch {

    [Obsolete("Obsolete API!", true)]
    public class PlayerIntent: Entry<INTENT> {

        protected override INTENT ValueProvider()
        {
            var brain = (graph as ModuleTree).Brain;
            return INTENT.PLAY;
            
            //return brain.getPlayerIntent();
        }

    }

}
