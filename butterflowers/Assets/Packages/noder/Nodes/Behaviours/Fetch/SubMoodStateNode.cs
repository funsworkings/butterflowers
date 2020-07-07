using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

namespace Noder.Nodes.Behaviours.Fetch 
{

    public class SubMoodStateNode: Entry<Brain.Sub_MoodState> {

        protected override Brain.Sub_MoodState ValueProvider()
        {
            return (graph as ModuleTree).Brain.getSubMoodState();
        }

    }

}
