using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.Networking.PlayerConnection;
using UnityEngine;

using XNode;
using XNodeEditor;
using UEditor = UnityEditor;

namespace Noder.Nodes.External.Editor {

    [CustomNodeEditor(typeof(BaseDialogueNode))]
    public class BaseDialogueNodeEditor: NodeEditor {

        private BaseDialogueNode node;

        private int textAreaHeight = 8;

        public override void OnBodyGUI()
        {
            if (node == null) node = target as BaseDialogueNode;

            // Update serialized object's representation
            serializedObject.Update();

            OnNodePropertyGUI(new string[] { "m_body" } );
            OnDynamicPortListGUI();

            var style = new GUIStyle();
            style.wordWrap = true;
            style.normal.textColor = Color.white;
            style.padding.top = 10;

            var body = serializedObject.FindProperty("m_body");
            var body_new = UEditor.EditorGUILayout.TextArea(body.stringValue, style);

            body.stringValue = body_new;

            // Apply property modifications
            serializedObject.ApplyModifiedProperties();
        }
    }

}
