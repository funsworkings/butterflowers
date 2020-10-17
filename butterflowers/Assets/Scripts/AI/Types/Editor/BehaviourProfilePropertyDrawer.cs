using System;
using UnityEditor;
using UnityEngine;

namespace AI.Types.Editor
{
	using Editor = UnityEditor.Editor;
	/*
	[CustomPropertyDrawer(typeof(BehaviourProfile))]
	public class BehaviourProfilePropertyDrawer : PropertyDrawer
	{
		SerializedProperty profileProperty;
		SerializedProperty mappingsProperty;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			
			// Draw label
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var mappings = property.FindPropertyRelative("mappings");
			var offset = 0;
			
			for (int i = 0; i < mappings.arraySize; i++) {
				var rect = new Rect(position.x, position.y + offset, position.width, 67);
				var prop = mappings.GetArrayElementAtIndex(i);

				EditorGUI.PropertyField(rect, prop,  GUIContent.none);
				
				offset += 25+67;
			}
			
			// Set indent back to what it was
			EditorGUI.indentLevel = indent;
			
			EditorGUI.EndProperty();
		}
	}
	
	*/
}