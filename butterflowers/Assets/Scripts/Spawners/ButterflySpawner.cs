using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflySpawner : Spawner
{
    [SerializeField] new Camera camera;

    [SerializeField] float minDistance = 0f, maxDistance = 1f;

    protected override void Start()
    {
        Butterfly.Died += ResetButterfly;

        base.Start();
    }

    protected override void DecidePosition(ref Vector3 pos)
    {
        if (camera != null)
        {
            Vector3 offset = Vector3.zero, position = Vector3.zero;

            offset = new Vector3(Random.Range(0f, 1f),
                                     Random.Range(0f, 1f),
                                     Random.Range(minDistance, maxDistance));

            position = camera.ViewportToWorldPoint(offset);


            pos = position;
        }
        else
            base.DecidePosition(ref pos);
    }

    void ResetButterfly(Butterfly butterfly)
    {
        InstantiatePrefab(butterfly.gameObject);
        butterfly.Reset();
    }

    void OnDestroy()
    {
        Butterfly.Died -= ResetButterfly;
    }
}
