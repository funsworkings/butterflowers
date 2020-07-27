﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;
using UnityEngine.UI;
using UIExt.Extras;
using TMPro;
using System.IO;
using System.Linq;

public class Beacon: MonoBehaviour {

	#region External

	World Room;
    Nest Nest;

    #endregion

    #region Internal

    public enum Status 
    {
        NULL,

        UNKNOWN,
        COMFORTABLE,
        ACTIONABLE
    }

    public Status status => (Room == null)? Status.NULL:Room.FetchBeaconStatus(this);

	#endregion

	#region Events

	public static System.Action<Beacon> OnRegister, OnUnregister;
    public static System.Action<Beacon> Discovered, Hidden, Destroyed;

    #endregion

    #region Properties

    [SerializeField] ParticleSystem revealPS, discoveryPS, deathPS, auraPS;

    Interactable interactable;
    SimpleOscillate Oscillate;
    Material material;

    [SerializeField] GameObject pr_impactPS, pr_shinePS;

    #endregion

    #region Attributes

    bool active = false;

    [SerializeField] bool self = false;
    [SerializeField] bool m_discovered = false, m_visible = true, m_destroyed = false;

    [SerializeField] GameObject infoPrefab, info;
    public BeaconInfo beaconInfo;
    Tooltip infoTooltip;

    [SerializeField] string m_file = null;
    [SerializeField] FileSystemEntry m_fileEntry;

    public Vector3 origin = Vector3.zero;
    Vector3 warp_a, warp_b;

    bool warping = false;
    bool warp_nest = false;
    float warp_t = 0f;

    [SerializeField] AnimationCurve warpScaleCurve, normalScaleCurve;
    [SerializeField] float timeToWarp = 1f, heightOfWarp = 1f;
    [SerializeField] float timeToDie = 1.67f;

    public Type type = Type.None;
    public bool learning = false;

    #endregion

    #region Accessors

    public string file {
        get
        {
            return m_file;
        }
        set
        {
            m_file = value;
            UpdateInfo();
        }
    }

    public FileSystemEntry fileEntry {
        get
        {
            return m_fileEntry;
        }
        set
        {
            m_fileEntry = value;
            UpdateInfo();
        }
    }

    public bool discovered {
        get
        {
            return m_discovered;
        }
        set
        {
            if (value)
                Discover(false);
            else
                Hide(false);
        }
    }

    public bool visible {
        get
        {
            return m_visible;
        }
        set
        {
            m_visible = value;
        }
    }

    public bool destroyed {
        get
        {
            return m_destroyed;
        }
    }

    public float knowledge 
    {
        get
        {
            return (Room == null)? 1f:Room.FetchBeaconKnowledgeMagnitude(this);
        }
    }

    public bool overrideFocus => (Room == null)? false : Room.FOCUS;

    #endregion

    #region Monobehaviour callbacks

    void Awake() {
        interactable = GetComponent<Interactable>();
        Oscillate = GetComponent<SimpleOscillate>();
        material = GetComponentInChildren<MeshRenderer>().material;

        Room = World.Instance;
        Nest = Nest.Instance;
    }

    void Start() 
    {
        CreateInfo();
    }

    void Update()
    {
        active = (Nest.open && visible);

        if(Oscillate != null) Oscillate.enabled = !warping || destroyed || !active;

        if (!active) HideInfo();

        UpdateColor();

        if (warping) {
            var interval = 0f;

            Vector3 dir = (warp_b - warp_a);

            if (warp_t <= timeToWarp) {
                warp_t += Time.deltaTime;
                interval = Mathf.Clamp01(warp_t / timeToWarp);

                float h = Mathf.Sin(interval * Mathf.PI) * heightOfWarp;
                transform.position = warp_a + (dir * Mathf.Pow(interval, 4f)) + (Vector3.up * h);

                float sa = (warp_nest) ? 1f : 0f; float sb = (warp_nest) ? 0f : 1f;
                AnimationCurve curve = (warp_nest) ? warpScaleCurve : normalScaleCurve;

                float si = Mathf.Lerp(sa, sb, curve.Evaluate(interval));
                transform.localScale = Vector3.one * si;
            }
            else 
            {
                warping = false;
                if (warp_nest) Nest.ReceiveBeacon(this);
            }
        }
    }

    void OnEnable() 
    {
        interactable.onGrab += Activate;
        interactable.onHover += Hover;
        interactable.onUnhover += Unhover;

        Beacon.Discovered += CheckIfDiscovered;
    }

    void OnDisable() {
        interactable.onGrab -= Activate;
        interactable.onHover -= Hover;
        interactable.onUnhover -= Unhover;

        Beacon.Discovered -= CheckIfDiscovered;

        Unregister();
    }

    void OnDestroy()
    {
        DestroyInfo();
    }

    #endregion

    #region Internal

    public enum Type {
        None,
        Desktop,
        Wizard,
        External
    }

    public void Register()
    {
        if (OnRegister != null)
            OnRegister(this);
    }

    public void Unregister()
    {
        if (OnUnregister != null)
            OnUnregister(this);
    }

	#endregion

	#region Operations

	public void Appear()
    {
        //revealPS.Play();
    }

    public void Disappear()
    {
        //revealPS.Stop();
    }

    public bool Discover(bool events = true)
    {
        if (!Nest.open) return false;
        if (destroyed) return false;

        m_discovered = true;
        if (Discovered != null && events)
            Discovered(this);

        discoveryPS.Stop();

        var impact = Instantiate(pr_impactPS, transform.position, transform.rotation);
        impact.GetComponent<ParticleSystem>().Play();

        //var shine = Instantiate(pr_shinePS, transform.position, transform.rotation);
        //shine.GetComponent<ParticleSystem>().Play();

        if (self)
            Nest.AddBeacon(this);

        return true;
    }

    public bool Hide(bool events = true)
    {
        if (destroyed) return false;

        m_discovered = false;
        if (Hidden != null && events)
            Hidden(this);

        discoveryPS.Play();

        return true;
    }

    public bool Delete(bool events = true, bool particles = false)
    {
        if (destroyed) return false;
        m_destroyed = true;

        Unregister();

        HideInfo();
        if (Destroyed != null && events) {
            Destroyed(this);
        }
        
        StartCoroutine("Dying", particles);

        return true;
    }

    #endregion

    #region Death

    IEnumerator Dying(bool particles = false)
    {
        float t = 0f;
        float sp = Oscillate.speed;

        if (particles) 
        {
            deathPS.Play();
            deathPS.transform.parent = null;

            yield return new WaitForSeconds(timeToDie);
        }
        
        Destroy(gameObject);
    }

    #endregion

    #region Appearance

    void UpdateColor() 
    {
        if (material == null) return;

        float hue = 0f;
        float sat = 0f, t_sat = (Nest.open) ? 1f : 0f;
        float val = 0f;

        Color.RGBToHSV(material.color, out hue, out sat, out val);
        Color actual = Color.HSVToRGB(hue, Mathf.Lerp(sat, t_sat, Time.deltaTime), val);
        actual.a = material.color.a;
        
        material.color = actual;
    }

	#endregion

	#region Interactable callbacks

	void Activate(Vector3 point, Vector3 normal){
        if (!active) return;

        bool success = Discover();
        if(success)
            Events.ReceiveEvent(EVENTCODE.BEACONACTIVATE, AGENT.User, AGENT.Beacon, details: file);
    }

    void Hover(Vector3 point, Vector3 normal){
        if (!active) return;
        DisplayInfo();
    }

    void Unhover(Vector3 point, Vector3 normal){
        if (!active) return;
        HideInfo();
    }

    public void OverrideHover()
    {
        DisplayInfo(AGENT.Wizard);
    }

    public void ClearOverrideHover()
    {
        HideInfo();
    }

    #endregion

    #region Beacon callbacks

    void CheckIfDiscovered(Beacon beacon)
    {
        if (beacon == null || beacon == this) return;

        if (beacon.file == file) { // Matches file
            Discover(false); // No fire events
        }
    }

	#endregion

	#region Warp operations

	public void WarpFromTo(bool nest = false)
    {
        interactable.enabled = !nest;

        warp_t = 0f;
        warp_nest = nest;

        warp_a = transform.position;
        warp_b = (nest) ? Nest.Instance.transform.position : origin;

        warping = true;
    }

	#endregion

	#region Info operations

	void CreateInfo(){
        var container = GameObject.FindGameObjectWithTag("Beacon Container");

        info = Instantiate(infoPrefab, container.transform);
        beaconInfo = info.GetComponent<BeaconInfo>();
        
        infoTooltip = info.GetComponent<Tooltip>();
        infoTooltip.target = transform;
        infoTooltip.camera = (Room == null)? Camera.main:Room.PlayerCamera;
        
        UpdateInfo();

        if(overrideFocus)
            DisplayInfo(AGENT.Wizard);
        else
            HideInfo();
    }

    void DestroyInfo()
    {
        if (info != null) {
            GameObject.Destroy(info);
            info = null;
        }
    }

    void DisplayInfo(AGENT agent = AGENT.User){
        if (destroyed) return;
        if (beaconInfo == null)
            return;

        beaconInfo.Show(agent);
    }

    void UpdateInfo()
    {
        if (beaconInfo == null)
            return;

        beaconInfo.beacon = this;
    }

    void HideInfo(){
        if(beaconInfo == null)
            return;

        beaconInfo.Hide();
    }

	#endregion
    
    #region Deprecated

    /*

    [SerializeField] MeshRenderer thumbnailPanel;
    
    [SerializeField] Texture2D m_thumbnail;
    public Texture2D thumbnail
    {
        set
        {
            m_thumbnail = value;
            onUpdateThumbnail();
        }
    }

    void onUpdateThumbnail()
    {
        if (thumbnailPanel == null)
            return;

        thumbnailPanel.material.mainTexture = m_thumbnail;
    }

    void OnDestroy()
    {
        Texture2D.Destroy(m_thumbnail); // Dispose thumbnail image on destroy
    }

    */

    #endregion
}
