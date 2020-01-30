using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class Assets
{
    public static T CreateOrReplaceAsset<T> (T asset, string path) where T:Object{
          T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
          
          if (existingAsset == null){
              AssetDatabase.CreateAsset(asset, path);
              existingAsset = asset;
          }
          else{
              EditorUtility.CopySerialized(asset, existingAsset);
          }
          
          return existingAsset;
     }

     public static void EnsureFolder(string path){
         bool exists = AssetDatabase.IsValidFolder(path);
         if(!exists){
            string[] subs = path.Split('/');
            string self = subs[subs.Length-1];

            string root = path.Substring(0, path.Length - (self.Length+1));

            AssetDatabase.CreateFolder(root, self); // Build folder out for sub-objects if not exists
         }
     }
}
