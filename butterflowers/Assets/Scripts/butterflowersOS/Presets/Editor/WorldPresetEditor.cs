using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using butterflowersOS.Core;
using UnityEditor;
using UnityEngine;
using uwu.Utilities.Editor;

namespace butterflowersOS.Presets
{
	[CustomEditor(typeof(WorldPreset))]
	public class WorldPresetEditor : Editor
	{
		SerializedObject _serializedObject;
		SerializedProperty _worldTexturesProperty;

		const string worldTexturesPath = "Assets/Textures/World";
		const string degradedWorldTexturesPath = "Assets/Textures/World_Export";
		
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
				List<string> previousExports = new List<string>();
				
				string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { degradedWorldTexturesPath });
				foreach (string guid in guids) 
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					previousExports.Add(path);
				}

				foreach (Texture2D tex in textures) 
				{
					string path = AssetDatabase.GetAssetPath(tex);
					
					if(!previousExports.Contains(path)) WriteDegradedTexture(tex);
					else Debug.LogWarning("Ignore texture write operation for " + tex.name);
				}
				Texture2D[] exportTextures = LoadAllExportTextures();
				
				int stackHeight = exportTextures.Length;
				_worldTexturesProperty.arraySize = stackHeight;
				
				for (int i = 0; i < stackHeight; i++) 
				{
					string assetPath = AssetDatabase.GetAssetPath( exportTextures[i] );
					var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
					if ( tImporter != null )
					{
						tImporter.textureType = TextureImporterType.Default;
						tImporter.isReadable = true; // Ensure texture is readable

						AssetDatabase.ImportAsset( assetPath );
						AssetDatabase.Refresh();
					}
					
					var prop = _worldTexturesProperty.GetArrayElementAtIndex(i);
					prop.objectReferenceValue = exportTextures[i]; // Set texture in array
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
		
		Texture2D[] LoadAllExportTextures()
		{
			List<Texture2D> textures = new List<Texture2D>();
			
			string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { degradedWorldTexturesPath });
			foreach (string guid in guids) 
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				Texture2D asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				
				textures.Add(asset);
			}

			return textures.ToArray();
		}

		void WriteDegradedTexture(Texture2D texture)
		{
			Assets.EnsureFolder(degradedWorldTexturesPath);
			
			string assetPath = AssetDatabase.GetAssetPath( texture );
			var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
			if ( tImporter != null )
			{
				tImporter.textureType = TextureImporterType.Default;
				tImporter.isReadable = true; // Ensure texture is readable

				AssetDatabase.ImportAsset( assetPath );
				AssetDatabase.Refresh();
			}
			
			string path = Path.Combine(degradedWorldTexturesPath, texture.name);
			Texture2D degradedTexture = Library.DegradeTexture(texture);

			byte[] bytes = degradedTexture.EncodeToJPG();
			path += ".jpg";
			File.WriteAllBytes( path, bytes);
		}
	}
}