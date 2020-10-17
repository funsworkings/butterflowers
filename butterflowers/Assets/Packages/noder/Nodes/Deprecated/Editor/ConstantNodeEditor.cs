using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using XNodeEditor;

namespace Noder.Nodes.Abstract
{
    [CustomNodeEditor(typeof(Constant<>))]
    public class ConstantNodeEditor<E> : NodeEditor
    {
        Constant<E> constantNode;

        public override void OnHeaderGUI() {
            if(constantNode == null) 
                constantNode = (target as Constant<E>);
            else 
                constantNode.Description = GUILayout.TextField(constantNode.Description, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
        }
    }
}