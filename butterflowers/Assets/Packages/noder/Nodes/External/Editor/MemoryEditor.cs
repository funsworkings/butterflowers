using Noder.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.Networking.PlayerConnection;
using UnityEngine;

using XNode;
using XNodeEditor;
using UEditor = UnityEditor;
using WMemory = Wizard.Memory;

namespace Noder.Nodes.External.Editor {

    [CustomNodeEditor(typeof(Memory))]
    public class BaseDialogueEditor: NodeEditor {

        private Memory node;
        private Texture2D node_tex;

        private int textureWidth = 64, width = 72, textAreaHeight = 72;
        private Vector2 scroll;

        private GUIStyle editorstyle;

        public override void OnBodyGUI()
        {
            if (node == null) node = target as Memory;

            if (editorstyle == null)
                editorstyle = new GUIStyle(EditorStyles.label);

            if (node.isActive)
                EditorStyles.label.normal.textColor = Color.yellow;

            // Update serialized object's representation
            serializedObject.Update();

            OnNodePropertyGUI(null);
            OnDynamicPortListGUI();

            var mem = serializedObject.FindProperty("memory");

            WMemory reference = null;
            string name = "NULL";
            string body = "";
            AudioClip audio = null;
            Texture2D image = null;
            float weight = 0f;

            if (mem != null) {
                var mem_o = mem.serializedObject;
                reference = mem.objectReferenceValue as WMemory;

                if (reference != null) {
                    node_tex = null;

                    body = reference.body;
                    image = reference.image;
                    audio = reference.audio;
                    weight = reference.weight;

                    // Draw property fields

                    float w = textureWidth;
                    float h = textureWidth;

                    if (image != null) {
                        float aspect = (float)image.height / image.width;
                        h = aspect * w;

                        name = image.name; // Used for lookup
                    }

                    var style = new GUIStyle();
                    style.fontStyle = FontStyle.BoldAndItalic;
                    style.fontSize = 12;

                    UEditor.EditorGUILayout.LabelField(name, style);

                    body = UEditor.EditorGUILayout.TextArea(body, GUILayout.Height(textAreaHeight));
                    audio = (AudioClip)UEditor.EditorGUILayout.ObjectField(audio, typeof(AudioClip), false);
                    image = (Texture2D)UEditor.EditorGUILayout.ObjectField(image, typeof(Texture2D), false, GUILayout.Width(w), GUILayout.Height(h));
                    weight = UEditor.EditorGUILayout.Slider(weight, -1f, 1f);

                    SerializedObject obj = new SerializedObject(reference);
                    obj.FindProperty("body").stringValue = body;
                    obj.FindProperty("image").objectReferenceValue = image;
                    obj.FindProperty("audio").objectReferenceValue = audio;
                    obj.FindProperty("weight").floatValue = weight;

                    obj.ApplyModifiedPropertiesWithoutUndo();
                }
                else 
                {
                    node_tex = (Texture2D)UEditor.EditorGUILayout.ObjectField(node_tex, typeof(Texture2D), false);
                    if (node_tex != null) 
                    {
                        if (GUILayout.Button("Create")) {
                            WMemory wmem = new WMemory();
                            wmem.image = node_tex;

                            WMemory inst = Assets.CreateScriptableObjectFromInstance<WMemory>("Assets/Presets/Memories", wmem.name, wmem);
                            ScriptableObject.Destroy(wmem);

                            serializedObject.FindProperty("memory").objectReferenceValue = inst;
                            (node.Graph as DialogueTree).AddMemoryToDatabase(inst);
                        }
                    }
                }

                EditorStyles.label.normal = editorstyle.normal;
            }
            

            // Apply property modifications
            serializedObject.ApplyModifiedProperties();
        }

        WMemory CreateMemory(Texture2D tex)
        {
            return null;
        }
    }

}
