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

	public static System.Action<Beacon> Discovered, Hidden, Destroyed;

    #endregion

    #region Properties

    new ParticleSystem particleSystem;
    ParticleSystem.TrailModule psTrails;

    Interactable interactable;
    SimpleOscillate Oscillate;

    #endregion

    #region Attributes

    [SerializeField] bool m_discovered = false, m_visible = false;

    [SerializeField] GameObject infoPrefab, info;
    Tooltip infoTooltip;
    TMP_Text infoText;

    [SerializeField] string m_file = null;
    [SerializeField] FileSystemEntry m_fileEntry;

    public Vector3 origin = Vector3.zero;
    bool warping = false;

    [SerializeField] float timeToWarp = 1f, heightOfWarp = 1f;

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

    #endregion

    #region Monobehaviour callbacks

    void Awake() {
        interactable = GetComponent<Interactable>();
        particleSystem = GetComponent<ParticleSystem>();
        Oscillate = GetComponent<SimpleOscillate>();
    }

    void Start() {
        psTrails = particleSystem.trails;

        Room = Manager.Instance;
        Nest = Nest.Instance;

        CreateInfo();
    }

    void Update()
    {
        Oscillate.enabled = !warping;
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
    }

    #endregion

    #region Operations

    public void Discover(bool events = true)
    {
        m_discovered = true;
        if (Discovered != null && events)
            Discovered(this);

        particleSystem.Stop();
    }

    public void Hide(bool events = true)
    {
        m_discovered = false;
        if (Hidden != null && events)
            Hidden(this);

        particleSystem.Play();
    }

    public void Destroy(bool events = true)
    {
        if (Destroyed != null && events)
            Destroyed(this);

        Destroy(gameObject); // Immediately destroy instance
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
        if(infoText == null)
            return;

        infoText.text = fileEntry.ShortName;
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
