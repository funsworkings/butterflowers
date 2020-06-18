using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.External {

    public class TemporaryDialogue : Dialogue, IReset
    {
        public bool visited = false;

        public override void Enter()
        {
            if (!visited) 
            {
                base.Enter();
                visited = true;
            }
            else 
            {
                (graph as Graph).activeNode = this;
                Next();
            }
        }

        public void Reset()
        {
            visited = false;
        }
    }

}
