using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;
using UnityEngine.UI;
using UIExt.Extras;
using TMPro;
using System.IO;

public class Beacon: MonoBehaviour {

	#region External

	Manager Room;
    Nest Nest;

    #endregion

    #region Events

    public static System.Action<Beacon> OnRegister, OnUnregister;
    public static System.Action<Beacon> Discovered, Hidden, Destroyed;

    #endregion

    #region Properties

    [SerializeField] ParticleSystem revealPS, discoveryPS, deathPS;

    Interactable interactable;
    SimpleOscillate Oscillate;
    Material material;

    [SerializeField] GameObject pr_impactPS;

    #endregion

    #region Attributes

    bool active = false;

    [SerializeField] bool m_discovered = false, m_visible = true, m_destroyed = false;

    [SerializeField] GameObject infoPrefab, info;
    Tooltip infoTooltip;
    TMP_Text infoText;

    [SerializeField] string m_file = null;
    [SerializeField] FileSystemEntry m_fileEntry;

    public Vector3 origin = Vector3.zero;
    bool warping = false;

    [SerializeField] float timeToWarp = 1f, heightOfWarp = 1f;
    [SerializeField] float timeToDie = 1.67f;

    public Type type = Type.None;

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

    #endregion

    #region Monobehaviour callbacks

    void Awake() {
        interactable = GetComponent<Interactable>();
        Oscillate = GetComponent<SimpleOscillate>();
        material = GetComponentInChildren<MeshRenderer>().material;

        Room = Manager.Instance;
        Nest = Nest.Instance;
    }

    void Start() {
        CreateInfo();
    }

    void Update()
    {
        active = (Nest.open && visible);

        Oscillate.enabled = !warping || destroyed || !active;

        if (!active) HideInfo();

        UpdateColor();
    }

    void OnEnable() {
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

    public void Discover(bool events = true)
    {
        if (!Nest.open) return;
        if (destroyed) return;

        m_discovered = true;
        if (Discovered != null && events)
            Discovered(this);

        discoveryPS.Stop();

        var impact = Instantiate(pr_impactPS, transform.position, transform.rotation);
        impact.GetComponent<ParticleSystem>().Play();
    }

    public void Hide(bool events = true)
    {
        if (destroyed) return;

        m_discovered = false;
        if (Hidden != null && events)
            Hidden(this);

        discoveryPS.Play();
    }

    public void Delete(bool events = true, bool particles = false)
    {
        if (destroyed) return;
        m_destroyed = true;

        Unregister();

        HideInfo();
        if (Destroyed != null && events) {
            Destroyed(this);
        }
        
        StartCoroutine("Dying", particles);
    }

    #endregion

    #region Internal

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
        Discover();
    }

    void Hover(Vector3 point, Vector3 normal){
        if (!active) return;
        DisplayInfo();
    }

    void Unhover(Vector3 point, Vector3 normal){
        if (!active) return;
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

	public void WarpFromTo(Vector3 origin, Vector3 destination, bool nest = false)
    {
        interactable.enabled = !nest;

        if (warping) StopCoroutine("WarpToLocation");
        StartCoroutine(WarpToLocation(origin, destination, nest));
    }

    IEnumerator WarpToLocation(Vector3 a, Vector3 b, bool nest = false)
    {
        float t = 0f;
        float interval = 0f;

        Vector3 dir = (b - a);

        warping = true;
        while (t <= timeToWarp) {
            t += Time.deltaTime;
            interval = Mathf.Clamp01(t / timeToWarp);

            float h = Mathf.Sin(interval * Mathf.PI) * heightOfWarp;
            transform.position = a + (dir * Mathf.Pow(interval, 4f)) + (Vector3.up * h);

            float sa = (nest) ? 1f : 0f; float sb = (nest) ? 0f : 1f;
            transform.localScale = Vector3.one * Mathf.Lerp(sa, sb, Mathf.Pow(interval, 2f));

            yield return null;
        }
        warping = false;

        if (nest) Nest.ReceiveBeacon(this);
    }

	#endregion

	#region Info operations

	void CreateInfo(){
        var canvas = FindObjectOfType<Canvas>();

        info = Instantiate(infoPrefab, canvas.transform);
        infoTooltip = info.GetComponent<Tooltip>();
        infoTooltip.target = transform;
        infoText = info.GetComponent<TMP_Text>();
        
        HideInfo();
    }

    void DisplayInfo(){
        if (destroyed) return;
        if (infoText == null)
            return;

        UpdateInfo();
    }

    void UpdateInfo()
    {
        if (infoText == null)
            return;

        var txt = (fileEntry == null) ? file : fileEntry.ShortName;
        infoText.text = txt;
    }

    void HideInfo(){
        if(infoText == null)
            return;

        infoText.text = "";
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
