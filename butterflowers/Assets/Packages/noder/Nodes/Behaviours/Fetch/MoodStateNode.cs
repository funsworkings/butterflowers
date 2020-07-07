using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

namespace Noder.Nodes.Behaviours.Fetch 
{

    public class MoodStateNode: Entry<Brain.MoodState> {

        protected override Brain.MoodState ValueProvider()
        {
            return (graph as ModuleTree).Brain.getMoodState();
        }

    }

}
