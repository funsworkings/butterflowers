using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using uwu.UI.Behaviors.Visibility;
using uwu.UI.Extras;
using NativeInteractable = uwu.Gameplay.Interactable;

[RequireComponent(typeof(NativeInteractable))]
public class Interactable : Entity
{

    // Properties

    NativeInteractable m_interactable;
	[SerializeField] GameObject prTooltip;

    // Attributes

    [Header("Interaction")]
        [SerializeField] bool m_interactive = true;

    [Header("Tooltips")]
        [SerializeField] GameObject container;
        [SerializeField] bool useTooltip = true;
        [SerializeField] string m_defaultTooltipText = "";

        protected GameObject tooltipInstance;
        protected Tooltip tooltip;
        protected TMP_Text tooltipText;
        protected ToggleOpacity tooltipOpacity;

	#region Accessors

    public bool interactive 
    {
        get
        {
            return m_interactive && Sun.active;
        }
        set
        {
            m_interactive = value;
            m_interactable.enabled = value;
        }
    }

    public string defaultTooltipText {
        get
        {
            return m_defaultTooltipText;
        }
        set
        {
            m_defaultTooltipText = value;

            if (useTooltip) 
            {
                UpdateTooltipText(value);
            }
        }
    }

    public NativeInteractable interactable
    {
        get { return m_interactable; }
        set
        {
            if (m_interactable != value) 
            {
                UnsubscribeFromInteractableEvents();
                m_interactable = value;
                SubscribeToInteractableEvents();
            }
        }
    }

    #endregion

    #region Monobehaviour callbacks

    protected virtual void Awake()
    {
        m_interactable = GetComponent<NativeInteractable>();
        if (m_interactable == null)
            m_interactable = gameObject.AddComponent<NativeInteractable>();
    }

	// Start is called before the first frame update
	protected override void OnStart()
    {
        if (useTooltip) {
            CreateTooltip();
            tooltip.active = false;
        }
        
        SubscribeToInteractableEvents();

        interactive = m_interactive; // Set interactive behaviours
    }

    protected override void Update()
    {
        m_interactable.enabled = interactive;

        if (!interactive) 
        {
            if (useTooltip) 
            {
                if (tooltipOpacity != null && tooltipOpacity.Shown) // Hide tooltip if still visible
                    HideTooltip();
            }
        }
        
        base.Update();
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeFromInteractableEvents();

        if (useTooltip)
            DestroyTooltip();
    }

	#endregion
    
    #region Interactable event subscriptions

    void SubscribeToInteractableEvents()
    {
        interactable.onHover += onHover;
        interactable.onUnhover += onUnhover;

        interactable.onGrab += onGrab;
        interactable.onContinue += onContinue;
        interactable.onRelease += onRelease;
    }

    void UnsubscribeFromInteractableEvents()
    {
        interactable.onHover -= onHover;
        interactable.onUnhover -= onUnhover;

        interactable.onGrab -= onGrab;
        interactable.onContinue -= onContinue;
        interactable.onRelease -= onRelease;
    }
    
    #endregion

	#region Tooltips

	protected virtual void CreateTooltip(Transform container = null)
    {
        if (container != null)
            this.container = container.gameObject;
        else {
            if (this.container == null) {
                var containerOption = GameObject.FindGameObjectWithTag("Tooltips");
                if (containerOption != null)
                    this.container = containerOption;
            }
        }
        
        tooltipInstance = Instantiate(prTooltip, this.container.transform);

        tooltip = tooltipInstance.GetComponent<Tooltip>();
            tooltip.target = transform;
            tooltip.camera = (World == null) ? Camera.main : World.PlayerCamera;

        tooltipText = tooltipInstance.GetComponentInChildren<TMP_Text>();
        tooltipOpacity = tooltipInstance.GetComponent<ToggleOpacity>();

        UpdateTooltipText(defaultTooltipText);
        HideTooltip();
    }

    protected virtual void DestroyTooltip()
    {
        GameObject.Destroy(tooltipInstance);
    }

    protected virtual void UpdateTooltipText(string text)
    {
        if(tooltip != null)
            tooltipText.text = text;
    }

    protected virtual void ShowTooltip()
    {
        tooltipOpacity.Show();
        tooltip.active = true;
    }

    protected virtual void HideTooltip()
    {
        tooltipOpacity.Hide();
        tooltip.active = false;
    }

    #endregion

    #region Interactable callbacks

    protected virtual void onHover(Vector3 point, Vector3 normal)
    {
        if(useTooltip)
            ShowTooltip();
    }

    protected virtual void onUnhover(Vector3 point, Vector3 normal)
    {
        if(useTooltip)
            HideTooltip();
    }

    protected virtual void onGrab(Vector3 point, Vector3 normal)
    {

    }

    protected virtual void onContinue(Vector3 point, Vector3 normal)
    {

    }

    protected virtual void onRelease(Vector3 point, Vector3 normal)
    {

    }

    #endregion
}
