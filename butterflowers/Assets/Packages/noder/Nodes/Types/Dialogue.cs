using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.Types {

    public class Dialogue: State {

        [TextArea(2, 30)] public string body = null;

        protected override void OnEnter()
        {
            DialogueTree tree = (graph as DialogueTree);
            if (tree != null) {

                DialogueHandler handler = tree.dialogueHandler;
                if (handler != null)
                    handler.Push(body);

                base.OnEnter();
            }
        }

    }

}
