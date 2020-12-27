using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Obsolete API!", true)]
public class Builder : MonoBehaviour
{
    public System.Action<Vector3> onBuildInDirection;


    [SerializeField] GameObject roomPrefab;
    [SerializeField] GameObject origin = null;

    [SerializeField] Vector3 bounds;
    [SerializeField] LayerMask overlapMask;


    void Start() {
        //Build(Vector3.zero, true);
        CalculateBounds();
    }

    void CalculateBounds()
    {
        Collider col = origin.GetComponentInChildren<Collider>();
        bounds = col.bounds.extents * 2f;
    }

    public void Build(Vector3 direction, bool force = false)
    {
        direction = (direction.normalized * bounds.x);
        transform.position += direction;

        if(onBuildInDirection != null) 
            onBuildInDirection.Invoke(transform.position);

        if (!force)
        {
            bool exists = RoomExistsAtLocation(transform.position);
            if (exists)
                return;
        }

        var room = Instantiate(roomPrefab, transform.position, roomPrefab.transform.rotation) as GameObject;
        if (origin == null)
            origin = room;
    }


    bool RoomExistsAtLocation(Vector3 position)
    {
        Collider[] collision = Physics.OverlapBox(position, bounds / 3f, roomPrefab.transform.rotation, overlapMask.value);
        return collision.Length > 0;
    }
}
