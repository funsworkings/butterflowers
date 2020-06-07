using System.Collections;
using System.Collections.Generic;
using Noder.Nodes.Types;
using UnityEngine;

using XNode;

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
       
    }

}
