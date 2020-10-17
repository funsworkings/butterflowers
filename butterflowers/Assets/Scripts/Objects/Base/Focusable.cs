﻿using Noder.Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.UIElements;
using uwu.Camera;
using uwu.Camera.Instances;

public class Focusable : Interactable
{
    // Events

    public UnityEvent onFocused, onLostFocus, onQueue;

    public static System.Action<Focusable> FocusOnPoint;
    public System.Action onFocus, onLoseFocus;

    // Attributes

    [SerializeField] bool focused = false, queued = false;
    public bool dispose = true;

    string m_message = null;

    // Properties
    
    [SerializeField] protected FocusCamera overrideCamera = null;

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
            if (Input.GetKeyDown(KeyCode.M))
                Focus();
        }
    }

    public void Focus()
    {
        if (focused) return;
        focused = true;

        Debug.Log("Focus on " + gameObject.name);

        onFocused.Invoke();
        if (onFocus != null)
            onFocus();

        if (FocusOnPoint != null)
            FocusOnPoint(this);
    }

    public void LoseFocus() {
        if (!focused) return;
        focused = false;

        onLostFocus.Invoke();
        if (onLoseFocus != null)
            onLoseFocus();
    }

    #region Interactable callbacks

    protected override void onHover(Vector3 point, Vector3 normal) {
        if (isFocused) 
        {
            ClearInfo();	
        }
        else 
        {
            AddInfo();
        }
        
        
        queued = true;
        onQueue.Invoke();
        
        base.onHover(point, normal);
    }

    protected override void onUnhover(Vector3 point, Vector3 normal) {
        queued = false;

        base.onUnhover(point, normal);
    }

    #endregion
    
    #region Info

    void AddInfo()
    {
        if (!InfoContainsFocusText()) {
            string message = tooltipText.text;

            if (!string.IsNullOrEmpty((message)))
                message += "\n";
            
            message += (this.message);
            UpdateTooltipText(message);
        }
    }

    void ClearInfo()
    {
        if (InfoContainsFocusText()) {
            string message = tooltipText.text;
			
            var index = message.IndexOf(this.message);
            message = message.Remove(index);

            UpdateTooltipText(message);
        }
    }

    bool InfoContainsFocusText()
    {
        return tooltipText.text.Contains(message);
    }
	
    #endregion
}
