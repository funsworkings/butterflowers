using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : MonoBehaviour
{
    Wand wand;
    IK ik;

    #region Monobehaviour callbacks

    void Awake()
    {
        ik = GetComponent<IK>();
    }

    // Start is called before the first frame update
    void Start()
    {
        wand = FindObjectOfType<Wand>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLookAt();
    }

    #endregion

    void UpdateLookAt()
    {
        if (ik == null)
            return;

        ik.lookAtWeight = (wand.spells) ? 1f : 0f;
        ik.lookAtPosition = (wand.worldPosition);
    }
}
