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
        int w = 0;
        int h = 0;


    public Texture2D[] textures;
    public int index = 0;

    // Start is called before the first frame updaste
    void Start()
    {
        w = canvas.width;
        h = canvas.height;

        sampler = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
        textures = new Texture2D[] { 
            new Texture2D(2, 2),   
            new Texture2D(2, 2),
            new Texture2D(2, 2),
            new Texture2D(2, 2)
        };

        ApplyTextures();

        Uploader.onSuccessReceiveImage += PushTexture;  

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

        RenderTexture.active = render;
    }

    public Color GetColorFromCanvas(Vector3 viewport){
        var x = Mathf.FloorToInt(viewport.x * w);
        var y = Mathf.FloorToInt(viewport.y * h);

        return sampler.GetPixel(x, y);
    }

    void PushTexture(byte[] dat){
        var tex = textures[index];
        tex.LoadImage(dat);

        ApplyTextures();

        if(++index > textures.Length-1)
            index = 0;
    }

    void ApplyTextures(){
        if(material == null)
            return;

        int i = 0;
        foreach(Texture2D t in textures){
            material.SetTexture(string.Format("_Tex{0}", i+1), textures[i]);
            ++i;
        }
    }

    void OnDestroy() {
        Uploader.onSuccessReceiveImage -= PushTexture;    
    }
}
