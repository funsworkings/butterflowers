using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnableEvents : MonoBehaviour
{
    public UnityEvent Enabled, Disabled;

    void OnEnable() { Enabled.Invoke(); }
    void OnDisable() { Disabled.Invoke(); }
}
