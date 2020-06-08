using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.External;

namespace Noder.Graphs {

    [CreateAssetMenu(fileName = "New Noder Dialogue Tree", menuName = "Noder/Graphs/Dialogue Tree", order = 53)]
    public class DialogueTree: Graph {

        DialogueHandler m_dialogueHandler = null;
        public DialogueHandler dialogueHandler {
            get
            {
                return m_dialogueHandler;
            }
            set
            {
                m_dialogueHandler = value;
            }
        }

        public void Step(int value)
        {
            if (activeNode != null) {
                (activeNode as BaseDialogueNode).Next(value);
                return;
            }

            Step();
        }

    }

}
