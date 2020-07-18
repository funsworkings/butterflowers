using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine.UI;

public class Quilt : MonoBehaviour 
{
    public static Quilt Instance = null;

    #region Events

    public System.Action<string, Texture> onLoadTexture;
    public System.Action<Texture> onDisposeTexture;

    #endregion

    #region External

    [SerializeField] Nest Nest;

	#endregion

	[SerializeField] Material material = null;
    [SerializeField] CreateTextureArray textureArray;
    
    [SerializeField] float m_speed = 0f;
    [SerializeField] float minLerpSpeed = 0f, maxLerpSpeed = 1f;

    [SerializeField] float offset = 0f;

    [SerializeField] float interval = 0f;
    public float speedInterval {
        get
        {
            interval = (m_speed - minLerpSpeed) / (maxLerpSpeed - minLerpSpeed);
            return interval;
        }
    }

    [SerializeField] float m_death = 0f, m_deathDecay = 1f;
    public float death {
        get
        {
            return m_death;
        }
        set
        {
            m_death = Mathf.Clamp01(value);
        }
    }

    public int textureCap = 4;

    [SerializeField] List<string> queue = new List<string>();
    [SerializeField] List<Texture2D> textures = new List<Texture2D>();

    int index = -1;

    private bool read = false, load = false;

    [SerializeField] RenderTexture canvas;

    [SerializeField] Texture2D debugTexture;

    [SerializeField]
    float refreshRate = .167f;

    Texture2D sampler;
    Color[] sample;

    int w = 0;
    int h = 0;

    #region Monobehaviour callbacks

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        if(Nest != null)
            Nest.onUpdateCapacity += UpdateTextureCap;
    }

    void OnDisable()
    {
        if(Nest != null)
            Nest.onUpdateCapacity -= UpdateTextureCap;
    }

    void Start(){
        if(Nest != null)
            textureCap = Nest.Instance.capacity;

        textures = new List<Texture2D>();

        SetTextureAttributes();
        ApplyTextures();
    }

    void Update()
    {
        if (death > 0f) 
        {
            death -= (Time.deltaTime * m_deathDecay);
        }

        material.SetFloat("_Interval", offset);
        material.SetFloat("_Death", (death > 0f)? 1f : 0f);
        material.SetInt("_TextureRange", textures.Count);

        UpdateLerpSpeed();
        if(wait <= 0f)
            offset = Mathf.Repeat(offset + (Time.deltaTime * m_speed), 1f);

        if(Nest != null)
            textureCap = Nest.capacity;
    }

    float wait = 0f;
    IEnumerator Wait()
    {
        while (wait > 0f) {
            wait = Mathf.Max(0f, wait - Time.deltaTime);
            yield return null;
        }

    }

    #endregion

    #region Operations

    public void Push(string file)
    {
        LoadTexture(file);
    }

    public void Pop(string file)
    {
        var _textures = textures.ToArray();
        for (int i = 0; i < _textures.Length; i++) 
        {
            var texture = _textures[i];
            if (texture.name == file) {
                Remove(texture);
                break;
            }
        }
    }

	public void Add(Texture tex){
        ++index;

        var texture = (tex as Texture2D);

        textures.Add(texture);
        ApplyTextures();
    }

    public void Remove(Texture tex) 
    {
        var texture = (tex as Texture2D);

        if (textures.Contains(texture)) {
            textures.Remove(texture);
            --index;

            if (onDisposeTexture != null)
                onDisposeTexture(texture);

            ApplyTextures();
        }

        death = 1f;
    }

    public void Dispose(bool apply = true) {
        var _textures = textures.ToArray();
        for (int i = 0; i < _textures.Length; i++) {
            if (onDisposeTexture != null)
                onDisposeTexture(_textures[i]);
        }

        index = 0;
        textures = new List<Texture2D>();

        if (apply) ApplyTextures();
    }

    #endregion

    #region Sample operations

    void SetTextureAttributes()
    {
        w = canvas.width;
        h = canvas.height;

        sampler = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
        Sample();

        StartCoroutine("Refresh");
    }

    IEnumerator Refresh()
    {
        while (true) {
            yield return new WaitForEndOfFrame();
            Sample();

            yield return new WaitForSeconds(refreshRate);
        }
    }

    void Sample()
    {
        var render = RenderTexture.active;

        RenderTexture.active = canvas;

        sampler.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
        sampler.Apply();

        sample = sampler.GetPixels();
        RenderTexture.active = render;
    }

    // BL = (0,0) TR = (1,1)

    public Color GetColorFromCanvas(Vector3 viewport)
    {
        var vx = viewport.x;
        var vy = viewport.y;

        if (vx < 0f || vx > 1f || vy < 0f || vy > 1f)
            return Color.black;

        var x = Mathf.FloorToInt(vx * w);
        var y = Mathf.FloorToInt((1f - vy) * h); // Invert y position to match uv space

        var ind = Mathf.FloorToInt((1f - vy) * w) + x;//Debug.Log("x: " + x + "  y: " + y + "index: " + ind + "  cap: " + sample.Length);
        return sampler.GetPixel(x, y);
    }

    #endregion

    #region Texture operations

    void LoadTexture(string path)
    {
        queue.Add(path);

        if (!load) {
            StartCoroutine("LoadFromQueue");
            load = true;
        }
    }

    IEnumerator LoadFromQueue()
    {
        while (queue.Count > 0) 
        {
            if (!read) {
                string file = queue[0];
                queue.RemoveAt(0);

                read = true;
                
                ReadBytes(file);
            }

            yield return null;
        }

        load = false;
    }

    async Task ReadBytes(string file)
    {
        var www = await new WWW(string.Format("file://{0}", file));

        read = false;

        if (!string.IsNullOrEmpty(www.error)) {
            throw new System.Exception("Error reading from texture");
        }

        Debug.Log("Success load = " + file);
        var texture = www.texture;
        texture.name = file;

        Add(texture);

        if (onLoadTexture != null)
            onLoadTexture(file, texture);
    }

    #endregion

    #region Internal

    void ApplyTextures(){
        if(material == null)
            return;

        textureArray.PopulateTextureArray(textures.ToArray());
        UpdateLerpSpeed();

        offset = 1f; // Snap back immediately to last added

        if (wait <= 0f) {
            wait = 1.67f;
            StartCoroutine("Wait");
        }
    }
    
    void UpdateLerpSpeed(){
        float percent = (float)textures.Count / textureCap;
        m_speed = percent.Remap(0f, 1f, minLerpSpeed, maxLerpSpeed);

        material.SetFloat("_LerpSpeed", m_speed);
    }

    #endregion

    #region Nest callbacks

    public void UpdateTextureCap(int capacity)
    {
        textureCap = capacity;
    }

	#endregion
}