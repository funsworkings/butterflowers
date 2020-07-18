﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XNode;
using Noder.Nodes.External;
using System.Linq;
using Wizard;

namespace Noder.Graphs {

    [CreateAssetMenu(fileName = "New Noder Dialogue Tree", menuName = "Noder/Graphs/Dialogue Tree", order = 53)]
    public class DialogueTree: Graph {

        public static System.Action<DialogueTree, string> onReceiveDialogue;
        public static System.Action<DialogueTree> onCompleteTree;

        [SerializeField] DialogueTree m_externalTree;
        public DialogueTree externalTree => m_externalTree; 

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

        [SerializeField] Memories Memories;

        public void FlagTemporaryDialogue(int[] node_ids)
        {
            TemporaryDialogue[] temp = GetNodes<TemporaryDialogue>().ToArray();
            foreach (TemporaryDialogue t in temp) {
                int id = t.GetInstanceID();
                t.visited = (node_ids.Contains(id));
            }
        }

        public void FlagEndOfTree()
        {
            if (onCompleteTree != null)
                onCompleteTree(this);
        }

        public void AddMemoryToDatabase(Wizard.Memory mem)
        {
            Memories.Add(mem);
        }

        public void Step(int value)
        {
            if (activeNode != null) {
                (activeNode as BaseDialogueNode).Next(value);
                return;
            }

            Step();
        }

        public void Push(string dialogue)
        {
            if (string.IsNullOrEmpty(dialogue))
                return;

            if (onReceiveDialogue != null)
                onReceiveDialogue(this, dialogue);
        }
    }

}
