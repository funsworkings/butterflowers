﻿using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

namespace Noder.Nodes.Behaviours.Fetch 
{

    public class NestFillAmount: Entry<float> {

        protected override float ValueProvider()
        {
            return (graph as ModuleTree).Brain.Nest.fill;
        }

    }

}
