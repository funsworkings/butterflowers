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

    [SerializeField] bool m_focus = false, m_queued = false;

    #endregion

    #region Properties

    Interactable interactable;

    [SerializeField] protected FocusCamera overrideCamera = null;

	#endregion

	#region Accessors

    public bool focus {
        get
        {
            return m_focus;
        }
    }

    public new FocusCamera camera {
        get
        {
            return overrideCamera;
        }
    }

	#endregion

	void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

    protected virtual void Start() { }

    void OnEnable()
    {
        if (interactable == null) return;

        interactable.onHover += Hover;
        interactable.onUnhover += Unhover;
    }

    void OnDisable()
    {
        if (interactable == null) return;

        interactable.onHover -= Hover;
        interactable.onUnhover -= Unhover;
    }

    void Update()
    {
        if (m_focus) return;

        if (m_queued && Input.GetMouseButtonDown(0))
            Focus();
    }

    public void Focus()
    {
        m_focus = true;

        if (onFocus != null)
            onFocus();

        if (FocusOnPoint != null)
            FocusOnPoint(this);
    }

    public void LoseFocus() {
        m_focus = false;

        if (onLoseFocus != null)
            onLoseFocus();

        if (LostFocusOnPoint != null)
            LostFocusOnPoint(this);
    }

    #region Interactable callbacks

    void Hover(Vector3 point, Vector3 normal) {
        m_queued = true;
    }

    void Unhover(Vector3 point, Vector3 normal) {
        m_queued = false;
    }

    #endregion 
}
