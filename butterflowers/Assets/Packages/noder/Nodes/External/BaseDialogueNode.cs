﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.External {

    public abstract class BaseDialogueNode: Branch<int> {
        
        public abstract string body { get; }

        protected override void OnEnter()
        {
            DialogueTree tree = (graph as DialogueTree);
            if (tree != null) {
                tree.Push(body);
                base.OnEnter();
            }
        }

    }

}