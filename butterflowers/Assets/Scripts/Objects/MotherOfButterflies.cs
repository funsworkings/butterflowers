using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using Settings;

///<summary>Mother of all of the butterflies</summary>
public class MotherOfButterflies : Spawner
{
    public static MotherOfButterflies Instance = null;

    [SerializeField] 
    WorldPreset preset = null;

    #region External

    [SerializeField] new Camera camera;

    #endregion

    #region Attributes

    [SerializeField] float minDistance = 0f, maxDistance = 1f;

    #endregion

    #region Monobehaviour callbacks

    protected override void Awake()
    {
        Instance = this;
        amount = preset.amountOfButterflies;

        base.Awake();
    }

    // Start is called before the first frame updaste
    protected override void Start()
    {
        Butterfly.Died += ResetButterfly;

        base.Start(); 
    }

    protected override void Update(){
        base.Update();
        amount = preset.amountOfButterflies;
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        Butterfly.Died -= ResetButterfly;
    }

    #endregion

    #region Spawner overrides

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

    protected override void SetPrefabAttributes(GameObject instance, Vector3 position, Quaternion rotation)
    {
        Butterfly butterfly = instance.GetComponent<Butterfly>();
        butterfly.position = position;

        butterfly.transform.localScale = Vector3.zero;
        butterfly.transform.rotation = rotation;
    }

    #endregion
}
