using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>Mother of all of the butterflies</summary>
public class Mother : MonoBehaviour
{
    [SerializeField] RenderTexture canvas;
    [SerializeField] float refreshRate = .167f;

    public Material material;
    public Texture2D sampler;
    Color[] sample;

    int w = 0;
    int h = 0;


    public List<Texture2D> textures = new List<Texture2D>();

    public int textureCap = 4;
    public bool allowRepeatTextures = false;

    public CreateTextureArray textureArray;

    int index = -1;

    // Start is called before the first frame updaste
    void Start()
    {
        w = canvas.width;
        h = canvas.height;

        sampler = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
        textures = new List<Texture2D>();

        ApplyTextures();

        Sample();

        Navigator.onSuccessReceiveImage += PushTexture;  

        StartCoroutine("Refresh");   
    }

    IEnumerator Refresh(){
        while(true){
            yield return new WaitForEndOfFrame();
                Sample();

            yield return new WaitForSeconds(refreshRate);
        }
    }

    void Sample(){
        var render = RenderTexture.active;

        RenderTexture.active = canvas;
        
        sampler.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
        sampler.Apply();

        sample = sampler.GetPixels();
        RenderTexture.active = render;
    }

    // BL = (0,0) TR = (1,1)

    public Color GetColorFromCanvas(Vector3 viewport){
        var vx = viewport.x;
        var vy = viewport.y;

        if (vx < 0f || vx > 1f || vy < 0f || vy > 1f)
            return Color.black;

        var x = Mathf.FloorToInt(vx * w);
        var y = Mathf.FloorToInt((1f - vy) * h); // Invert y position to match uv space

        var ind = Mathf.FloorToInt((1f - vy) * w) + x;//Debug.Log("x: " + x + "  y: " + y + "index: " + ind + "  cap: " + sample.Length);
        return sampler.GetPixel(x, y);
    }

    void PushTexture(Texture tex){
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

    void ApplyTextures(){
        if(material == null)
            return;

        textureArray.PopulateTextureArray(textures.ToArray());
    }

    void OnDestroy() {
        Navigator.onSuccessReceiveImage -= PushTexture;    
    }
}
