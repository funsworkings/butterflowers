using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;
using UnityEngine.UI;
using UIExt.Extras;
using TMPro;

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

    [SerializeField] ParticleSystem revealPS, discoveryPS;

    Interactable interactable;
    SimpleOscillate Oscillate;

    #endregion

    #region Attributes

    [SerializeField] bool m_discovered = false, m_visible = false, m_destroyed = false;

    [SerializeField] GameObject infoPrefab, info;
    Tooltip infoTooltip;
    TMP_Text infoText;

    [SerializeField] string m_file = null;
    [SerializeField] FileSystemEntry m_fileEntry;

    public Vector3 origin = Vector3.zero;
    bool warping = false;

    [SerializeField] float timeToWarp = 1f, heightOfWarp = 1f;
    [SerializeField] float timeToDie = 1.67f;

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
    }

    void Start() {
        Room = Manager.Instance;
        Nest = Nest.Instance;

        CreateInfo();
    }

    void Update()
    {
        Oscillate.enabled = !warping || destroyed;
        //psTrails.enabled = warping;
    }

    void OnEnable() {
        interactable.onGrab += Activate;
        interactable.onHover += Hover;
        interactable.onUnhover += Unhover;
    }

    void OnDisable() {
        interactable.onGrab -= Activate;
        interactable.onHover -= Hover;
        interactable.onUnhover -= Unhover;

        Unregister();
    }

    #endregion

    #region Internal

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
        revealPS.Play();
    }

    public void Disappear()
    {
        revealPS.Stop();
    }

    public void Discover(bool events = true)
    {
        if (destroyed) return;

        m_discovered = true;
        if (Discovered != null && events)
            Discovered(this);

        discoveryPS.Stop();
    }

    public void Hide(bool events = true)
    {
        if (destroyed) return;

        m_discovered = false;
        if (Hidden != null && events)
            Hidden(this);

        discoveryPS.Play();
    }

    public void Delete(bool events = true)
    {
        if (destroyed) return;
        m_destroyed = true;

        HideInfo();
        if (Destroyed != null && events) {
            Destroyed(this);
        }

        Destroy(gameObject); //Destroy immediately
        //StartCoroutine("Dying");
    }

    #endregion

    #region Internal

    IEnumerator Dying()
    {
        float t = 0f;
        float sp = Oscillate.speed;

        while (t < timeToDie) {

            var offset = (timeToDie - t);
            var str = Mathf.Pow(offset, 2f);

            Oscillate.speed = sp * str;

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

	#endregion

	#region Interactable callbacks

	void Activate(Vector3 point, Vector3 normal){
        Discover();
    }

    void Hover(Vector3 point, Vector3 normal){
        DisplayInfo();
    }

    void Unhover(Vector3 point, Vector3 normal){
        HideInfo();
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
