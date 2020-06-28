using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class FocalPoint : MonoBehaviour
{
    #region Events

    public static System.Action<FocalPoint> FocusOnPoint, LostFocusOnPoint;
    public System.Action onFocus, onLoseFocus;

    #endregion

    #region Attributes

    [SerializeField] bool focused = false, queued = false, focusinprogress = false;
    
    [SerializeField] float timetofocus = 1f;
    float t_focustime = 0f;

    #endregion

    #region Properties

    Interactable interactable;

    [SerializeField] protected FocusCamera overrideCamera = null;

	#endregion

	#region Accessors

    public bool isFocused {
        get
        {
            return focused;
        }
    }

    public new FocusCamera camera {
        get
        {
            return overrideCamera;
        }
    }

    public bool isFocusing => focusinprogress;

	#endregion

	void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

    protected virtual void Start() { }

    void OnEnable()
    {
        if (interactable == null) return;

        focused = focusinprogress = queued =  false;

        interactable.onHover += Hover;
        interactable.onUnhover += Unhover;
        interactable.onGrab += Grab;
    }

    void OnDisable()
    {
        if (interactable == null) return;

        interactable.onHover -= Hover;
        interactable.onUnhover -= Unhover;
        interactable.onGrab -= Grab;
    }

    void Update()
    {
        if ((focused || !focusinprogress) || (focusinprogress && Input.GetMouseButtonUp(0))) 
        {
            t_focustime = 0f;
            focusinprogress = false;

            return;
        }

        if (t_focustime >= timetofocus) 
        {
            focusinprogress = false;
            t_focustime = 0f;

            Focus();
        }
        else
            t_focustime += Time.deltaTime;
    }

    public void Focus()
    {
        if (focused) return;
        focused = true;

        Debug.Log("Focus on " + gameObject.name);
        if (onFocus != null)
            onFocus();

        if (FocusOnPoint != null)
            FocusOnPoint(this);
    }

    public void LoseFocus() {
        if (!focused) return;
        focused = false;

        if (onLoseFocus != null)
            onLoseFocus();

        if (LostFocusOnPoint != null)
            LostFocusOnPoint(this);
    }

    #region Interactable callbacks

    void Hover(Vector3 point, Vector3 normal) {
        queued = true;
    }

    void Unhover(Vector3 point, Vector3 normal) {
        queued = false;
    }

    void Grab(Vector3 point, Vector3 normal)
    {
        if (!queued || isFocused) return;
        focusinprogress = true;
    }

    #endregion 
}
