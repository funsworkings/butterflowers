using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace uwu.Utilities.Editor
{
	public class Assets
	{
		#region Scriptable objects

		public static E CreateScriptableObjectFromInstance<E>(string path, string id, E instance) where E : ScriptableObject
		{
			if (string.IsNullOrEmpty(id)) throw new Exception("ID for scriptable object instance was not set!");

			var inst = ScriptableObject.CreateInstance<E>();
			inst = instance;

			var full_path = Path.Combine(path, string.Format("{0}.asset", id));
			return CreateOrReplaceAsset(inst, full_path);
		}

		#endregion

		#region Assets

		public static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
		{
			var existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

			if (existingAsset == null) {
				AssetDatabase.CreateAsset(asset, path);
				existingAsset = asset;
			}
			else {
				EditorUtility.CopySerialized(asset, existingAsset);
			}

			return existingAsset;
		}

		public static void EnsureFolder(string path)
		{
			var exists = AssetDatabase.IsValidFolder(path);
			if (!exists) {
				var subs = path.Split('/');
				var self = subs[subs.Length - 1];

				var root = path.Substring(0, path.Length - (self.Length + 1));

				AssetDatabase.CreateFolder(root, self); // Build folder out for sub-objects if not exists
			}
		}

		#endregion

		#region Prefabs

		public enum Status
		{
			Null = -1,
			Missing = 0,
			Same = 1,
			Overrides = 2
		}

		public static GameObject CreateOrReplacePrefab(GameObject o, string path)
		{
			var existing = (GameObject) AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
			GameObject prefab = null;

			var isPrefab = PrefabUtility.GetPrefabInstanceStatus(o) != PrefabInstanceStatus.NotAPrefab;
			// Prefab does not exist at location
			if (existing == null) {
				//if(isPrefab || PrefabUtility.IsPrefabAssetMissing(o))  // If already is prefab, unpack outer root (maintain child prefabs, if there are any)
				if (isPrefab)
					PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(o),
						PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
				prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(o, path, InteractionMode.UserAction);
			}
			else { // Prefab exists
				if (isPrefab) {
					var pr = PrefabUtility.GetCorrespondingObjectFromSource(o);
					if (pr == existing) {
						var overrides = PrefabUtility.HasPrefabInstanceAnyOverrides(o, true);
						if (overrides)
							PrefabUtility.ApplyPrefabInstance(o, InteractionMode.UserAction);

						prefab = pr;
					}
					else {
						AssetDatabase.DeleteAsset(path);
						PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(o),
							PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);

						prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(o, path, InteractionMode.UserAction);
					}
				}
				else {
					AssetDatabase.DeleteAsset(path);
					prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(o, path, InteractionMode.UserAction);
				}
			}

			AssetDatabase.Refresh();

			return prefab;
		}

		public static int CheckStatus(GameObject o)
		{
			var status = -1;

			if (o != null) {
				if (PrefabUtility.IsPartOfPrefabAsset(o))
					++status;
				if (!PrefabUtility.IsPrefabAssetMissing(o))
					++status;
				if (PrefabUtility.HasPrefabInstanceAnyOverrides(o, true))
					++status;
			}

			return status;
		}

		public static GameObject GetPrefabAtLocation(string path)
		{
			return (GameObject) AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
		}

		#endregion
	}
}