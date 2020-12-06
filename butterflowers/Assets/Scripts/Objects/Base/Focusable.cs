using Noder.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.UIElements;
using uwu.Camera;
using uwu.Camera.Instances;
using uwu.Extensions;

public class Focusable : Interactable
{
    // Events

    public UnityEvent onFocused, onLostFocus, onQueue;

    public static System.Action<Focusable> FocusOnPoint;
    public System.Action onFocus, onLoseFocus;

    // Attributes

    [SerializeField] bool focused = false, queued = false;
    [SerializeField] Transform anchor;

    public bool dispose = true;

    string m_message = null;

    // Properties
    
    [SerializeField] protected FocusCamera overrideCamera = null;

	#region Accessors

    public bool isFocused => focused;

    public new FocusCamera camera => overrideCamera;

    public Transform Anchor => anchor;
    
    string message
    {
        get
        {
            if (m_message == null) {
                var id = gameObject.name;
                m_message = string.Format("[ M ]ove to {0}", id);
            }

            return m_message;
        }
    }

    #endregion

    protected override void OnStart()
    {
        base.OnStart();

        focused = queued =  false;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        if (queued) {
            if (ExtraInput.GetScrollUp())
                Focus();
        }
    }

    public bool Focus()
    {
        if (focused) return false;
        focused = true;

        Debug.Log("Focus on " + gameObject.name);

        onFocused.Invoke();
        if (onFocus != null)
            onFocus();

        if (FocusOnPoint != null)
            FocusOnPoint(this);

        return true;
    }

    public bool LoseFocus() {
        if (!focused) return false;
        focused = false;

        onLostFocus.Invoke();
        if (onLoseFocus != null)
            onLoseFocus();

        return true;
    }

    #region Interactable callbacks

    protected override void onHover(Vector3 point, Vector3 normal) {
        queued = true;
        onQueue.Invoke();
        
        base.onHover(point, normal);
    }

    protected override void onUnhover(Vector3 point, Vector3 normal) {
        queued = false;

        base.onUnhover(point, normal);
    }

    #endregion
}
