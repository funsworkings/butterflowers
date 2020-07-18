using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.Abstract;
using Noder.Graphs;

namespace Noder.Nodes.External {

    public class Dialogue : BaseDialogueNode
    {
        [TextArea(2, 30)] public string m_body = null;
        public override string body => m_body;

        public bool flag = false;

        protected override void OnEnter()
        {
            DialogueTree tree = (graph as DialogueTree);
            if (tree != null) {
                base.OnEnter();

                if (flag)
                    tree.FlagEndOfTree();
            }
        }
    }

}
