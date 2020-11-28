﻿using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using AI.Types;
using UnityEngine;
using Wizard;

namespace Noder.Nodes.Behaviours.Fetch 
{

    public class MoodStateNode: Entry<Mood> {

        protected override Mood ValueProvider()
        {
            return (graph as ModuleTree).Brain.getMoodState();
        }

    }

}
