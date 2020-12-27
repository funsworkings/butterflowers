using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Obsolete API!", true)]
public class Follower : MonoBehaviour
{
    [SerializeField] Builder builder;


    [SerializeField] List<Vector3> locations = new List<Vector3>();
    [SerializeField] Vector3 offset = Vector3.zero;

    [SerializeField] bool lerping = false;
    [SerializeField] float timeToLerp = 1f;

    void Start()
    {
        offset = (transform.position - builder.transform.position);

        builder.onBuildInDirection += AddLocationToQueue;
    }

    void OnDestroy()
    {
        builder.onBuildInDirection -= AddLocationToQueue;
    }

    void AddLocationToQueue(Vector3 position)
    {
        locations.Add(position);
        if (!lerping)
        {
            StartCoroutine("MoveToLocations");
            lerping = true;
        }
    }

    IEnumerator MoveToLocations()
    {
        var t = 0f;
        var pos = Vector3.zero;
        var interval = 0f;
        var init = false;

        while(locations.Count > 0)
        {
            if (!init)
            {
                t = interval = 0f;
                pos = transform.position - offset;

                init = true;
            }

            var target = locations[0];
            interval = Mathf.Clamp01(t / timeToLerp);


            transform.position = Vector3.Lerp(pos, target, interval) + offset;

            if(interval >= 1f)
            {
                locations.RemoveAt(0);
                init = false;
            }
            else
                t += Time.deltaTime;

            yield return null;
        }

        lerping = false;
    }
}
