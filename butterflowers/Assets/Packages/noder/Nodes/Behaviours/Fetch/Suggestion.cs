using Noder.Graphs;
using Noder.Nodes.Abstract;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Noder.Nodes.Behaviours.Fetch {

    public class Suggestion: Entry<SUGGESTION> {

        protected override SUGGESTION ValueProvider()
        {
            var brain = (graph as ModuleTree).Brain;
            //return brain.getSuggestion();
            return SUGGESTION.NULL;
        }

    }

}
