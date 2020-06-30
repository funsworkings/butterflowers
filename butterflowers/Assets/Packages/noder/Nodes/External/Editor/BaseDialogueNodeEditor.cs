using Noder.Nodes.Abstract;
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
        private State state_node;

        private int textAreaHeight = 8;

        GUIStyle editorstyle;

        public override void OnBodyGUI()
        {
            if (node == null) 
                node = target as BaseDialogueNode;

            if (editorstyle == null) 
                editorstyle = new GUIStyle(EditorStyles.label);

            if (node.isActive)
                EditorStyles.label.normal.textColor = Color.yellow;

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

            EditorStyles.label.normal = editorstyle.normal;
        }
    }

}
