using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class ScalerBehaviour : PlayableBehaviour
{
    public Vector3 scale;
    public bool useX;
    public bool useY;
    public bool useZ;

    Vector3 baseScale;
    bool cache = false;

    public override void OnPlayableCreate (Playable playable)
    {
        
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform t = (Transform) playerData;
        if(t != null) 
        {
            Vector3 sc = t.localScale;
            
            if(!cache) 
            {
                baseScale = t.localScale;
                cache = true;
            }

            if (useX) sc.x = scale.x;
            if (useY) sc.y = scale.y;
            if (useZ) sc.z = scale.z;

            t.localScale = sc;
        }
    }
}
