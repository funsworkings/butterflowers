using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Networking;

public class Quilt : MonoBehaviour 
{
    public static Quilt Instance = null;

    [SerializeField] Material material = null;
    [SerializeField] CreateTextureArray textureArray;
    
    [SerializeField] float m_speed = 0f;
    [SerializeField] float minLerpSpeed = 0f, maxLerpSpeed = 1f;

    [SerializeField] int textureCap = 4;
    [SerializeField] bool allowRepeatTextures = false;

    List<Texture2D> textures = new List<Texture2D>();
    int index = -1;

    private bool read = false, load = false;

    List<string> queue = new List<string>();

    [SerializeField] RenderTexture canvas;

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

    void Start(){
        textures = new List<Texture2D>();

        SetTextureAttributes();
        ApplyTextures();
    }

	#endregion

	#region Operations

	public void Add(Texture tex){
        var texture = (tex as Texture2D);

        ++index;
        if (index == textureCap)
        {
            index = 0;

            var _textures = textures.ToArray();
            for (int i = 0; i < _textures.Length; i++)
                Texture2D.Destroy(textures[i]); // Destroy all temporary textures

            textures = new List<Texture2D>();
        }

        textures.Add(texture);
        ApplyTextures();
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

    public void LoadTexture(string path)
    {
        queue.Add(path);
        if (!load)
        {
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

                StartCoroutine("ReadBytesFromFile", file);
                read = true;
            }

            yield return null;
        }

        load = false;
    }

    IEnumerator ReadBytesFromFile(string file)
    {
        Debug.LogFormat("Quilt adding = {0}", file);

        UnityWebRequest req = UnityWebRequestTexture.GetTexture("file://" + file);
        req.SendWebRequest();

        while (!req.isDone) {
            Debug.Log(req.downloadProgress * 100f + "%");
            yield return null;
        }

        if (!(req.isHttpError || req.isNetworkError)) {
            Texture texture = ((DownloadHandlerTexture)req.downloadHandler).texture;
            Add(texture);
        }

        read = false;
    }

    #endregion

    #region Internal

    void ApplyTextures(){
        if(material == null)
            return;

        textureArray.PopulateTextureArray(textures.ToArray());
        UpdateLerpSpeed();
    }

    void UpdateLerpSpeed(){
        float percent = (float)textures.Count / textureCap;
        m_speed = percent.Remap(0f, 1f, minLerpSpeed, maxLerpSpeed);

        material.SetFloat("_LerpSpeed", m_speed);
    }

	#endregion
}