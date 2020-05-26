using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic class for options within a selection, send select events to selection
/// </summary>

public class Option<T> : MonoBehaviour
{
    public delegate void Selected(Option<T> option);
    public static event Selected onSelected;

    [SerializeField] protected string id;
    public string Id
    {
        get
        {
            return id;
        }
    }

    public int index = 0;

    [SerializeField] protected T attribute; // Data contained within option
    public virtual T Attribute
    {
        get
        {
            return attribute;
        }
        set
        {
            attribute = value;
        }
    }

    protected bool active = false;  // Currently selected?
    protected Selection<T> selection;

    public void Bind(Selection<T> selection)
    {
        this.selection = selection; // Assign selecto to communicate with
    }

    public virtual void Select()
    {
        //if(active) return;

        active = true;
        if (selection != null)
            selection.SelectOption(this);

        if (onSelected != null)
            onSelected(this);
    }

    public virtual void Deselect()
    {
        if (!active) return;

        active = false;
    }
}
