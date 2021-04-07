using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace butterflowersOS.Presets
{
	[CustomEditor(typeof(WorldPreset))]
	public class WorldPresetEditor : Editor
	{
		SerializedObject _serializedObject;
		SerializedProperty _worldTexturesProperty;

		const string worldTexturesPath = "Assets/Textures/World";
		List<string> ignore = new List<string>(){ "m_Script" };

		void OnEnable()
		{
			_serializedObject = serializedObject;
			_worldTexturesProperty = serializedObject.FindProperty("worldTextures");
		}

		public override void OnInspectorGUI()
		{
			_serializedObject.Update();

			var iterator = _serializedObject.GetIterator();
			if (iterator.Next(true)) 
			{
				do {

					if (!ignore.Contains(iterator.name)) 
					{
						EditorGUILayout.PropertyField(_serializedObject.FindProperty(iterator.name), true);
					}

				} while (iterator.Next(false));
			}

			if (GUILayout.Button("Fetch all world texture files!")) {

				Texture2D[] textures = LoadAllWorldTextures();
				int stackHeight = textures.Length;

				_worldTexturesProperty.arraySize = stackHeight;
				for (int i = 0; i < stackHeight; i++) {
					var prop = _worldTexturesProperty.GetArrayElementAtIndex(i);
					prop.objectReferenceValue = textures[i]; // Set texture in array
				}
			}

			_serializedObject.ApplyModifiedProperties();
		}

		Texture2D[] LoadAllWorldTextures()
		{
			List<Texture2D> textures = new List<Texture2D>();
			
			string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { worldTexturesPath });
			foreach (string guid in guids) 
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				Texture2D asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				
				textures.Add(asset);
			}

			return textures.ToArray();
		}
	}
}