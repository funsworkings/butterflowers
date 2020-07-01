﻿using Noder.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noder.Nodes.External 
{

    public class LinkedMemoryNode: Dialogue {

        public Memory memoryNode;

        protected override void OnEnter()
        {
            base.OnEnter();

            if (memoryNode != null) 
            {
                DialogueTree tree = (graph as DialogueTree);

                if (tree != null) 
                {
                    DialogueTree ext_tree = tree.externalTree;

                    if (ext_tree != null)
                        ext_tree.JumpToNode(memoryNode, true);
                }
            }
        }

    }

}
