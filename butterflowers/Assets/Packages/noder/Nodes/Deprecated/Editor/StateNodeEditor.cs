using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using XNodeEditor;

namespace Noder.Nodes.Abstract
{
    [CustomNodeEditor(typeof(State))]
    public class StateNodeEditor : NodeEditor
    {
        State stateNode;

        public override void OnBodyGUI() {
            base.OnBodyGUI();

            if(stateNode == null) 
                stateNode = (target as State);
            else { 
                if(stateNode.isActive)
                    EditorGUILayout.LabelField("*****\n*****\n*****\n*****\n*****");
            }
        }
    }
}


