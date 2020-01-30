using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Transforming
{
    public static void ChangeLayersRecursively(Transform trans, string name, bool self)
    {
        if(self) trans.gameObject.layer = LayerMask.NameToLayer(name);
        foreach (Transform child in trans)
        {
            child.gameObject.layer = LayerMask.NameToLayer(name);
            ChangeLayersRecursively(child, name, false);
        }
    }

    public static void CopyTransformValues(GameObject from, ref GameObject to, bool local){
        if(local){
            to.transform.localPosition = from.transform.localPosition;
            to.transform.localRotation = from.transform.localRotation;
            to.transform.localScale = from.transform.localScale;
        }
        else{
            to.transform.position = from.transform.position;
            to.transform.rotation = from.transform.rotation;
            to.transform.localScale = from.transform.localScale;
        }
        
    }
}
