using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflySpawner : Spawner
{
    [SerializeField] new Camera camera;


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

            offset = new Vector3(Random.Range(.33f, .67f),
                                     Random.Range(.33f, .67f),
                                     Random.Range(extents.z, 2f * extents.z));

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
