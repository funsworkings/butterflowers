using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic class for attributes that are driven by a controller (Player, Capture, etc)
/// </summary>

public abstract class Attribute<T> : MonoBehaviour where T:MonoBehaviour
{
    protected T controller;

    [SerializeField] protected bool active = true;
    public bool Active {
        get{
            return (active && (controller != null && controller.enabled));
        }
        set
        {
            active = value;

            if (active)
                onActive();
            else
                onInactive();
        }
    }


    protected virtual void onActive() { }
    protected virtual void onInactive() { }


    protected void Awake() {
        controller = GetComponent<T>(); // Fetch controller
        onAwake();
    }
    protected virtual void onAwake() { }


    protected void Start()
    {
        if (!Active)
            return;

        onStart();
    }
    protected virtual void onStart() { }


    protected void Update() {
        if(!Active)  // Halt all update commands if not active
            return;

        onUpdate();
    }
    protected virtual void onUpdate() { }


    protected void OnDestroy() {
        if (!Active)
            return;

        onDestroy();
    }
    protected virtual void onDestroy() { }


    public virtual void Enable(){
        active = true;
    }

    public virtual void Disable(){
        active = false;
    }

    public virtual void Toggle(){
        if(active) Disable();
        else Enable();
    }
}
