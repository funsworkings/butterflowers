using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class Prefabs
{
    public enum Status {
        Null      = -1,
        Missing   = 0,
        Same      = 1,
        Overrides = 2
    }

    public static GameObject CreateOrReplacePrefab(GameObject o, string path){
        GameObject existing = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        GameObject prefab = null;
        
        bool isPrefab = (PrefabUtility.GetPrefabInstanceStatus(o) != PrefabInstanceStatus.NotAPrefab);
        // Prefab does not exist at location
        if(existing == null){
            //if(isPrefab || PrefabUtility.IsPrefabAssetMissing(o))  // If already is prefab, unpack outer root (maintain child prefabs, if there are any)
            if(isPrefab) PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(o), PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
            prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(o, path, InteractionMode.UserAction);
        }
        else { // Prefab exists
            if(isPrefab) {
                GameObject pr = PrefabUtility.GetCorrespondingObjectFromSource(o);
                if(pr == existing){
                    bool overrides = PrefabUtility.HasPrefabInstanceAnyOverrides(o, true);
                    if(overrides)
                        PrefabUtility.ApplyPrefabInstance(o, InteractionMode.UserAction);

                    prefab = pr;
                }
                else {
                    AssetDatabase.DeleteAsset(path);
                    PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(o), PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
                    
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

    public static int CheckStatus(GameObject o){
        int status = -1;
        
        if(o != null){
            if(PrefabUtility.IsPartOfPrefabAsset(o))
                ++status;
            if(!PrefabUtility.IsPrefabAssetMissing(o))
                ++status;
            if(PrefabUtility.HasPrefabInstanceAnyOverrides(o, true))
                ++status;
        }

        return status;
    }

    public static GameObject GetPrefabAtLocation(string path){
        return (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
    }
}
