using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CreateTextureArray : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] Renderer rend;
    [SerializeField] List<Texture2D> textures = new List<Texture2D>();

    [SerializeField] Texture2D[] validated = new Texture2D[] { };

    [SerializeField] int m_width, m_height;

    public bool refresh = false;

    public bool hardLimitOnSize = true;

    public string texturePropertyId = "_MainTex";
    public string textureArrayCountPropertyId = "_MainTexCount";

    [SerializeField] 
    Texture2DArray array = null;

    [SerializeField]
    TextureWrapMode wrapMode = TextureWrapMode.Repeat;

    [SerializeField]
    FilterMode filterMode = FilterMode.Bilinear;
    

    // Start is called before the first frame update
    void Start()
    {
        onUpdateTextureArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (refresh)
        {
            onUpdateTextureArray();
            refresh = false;
        }
    }

    public void PopulateTextureArray(Texture2D[] _textures)
    {
        textures.Clear();
        foreach (Texture2D tex in _textures)
        {
            //tex.Compress(true);
            textures.Add(tex);
        }

        refresh = true;
    }

    void onUpdateTextureArray()
    {
        if(material == null)
        {
            Debug.LogWarning("Material has not been set for texture array creator!");
            return;
        }

        if (array != null)
            Texture2DArray.DestroyImmediate(array); // Clear out previous texture if not null


        List<Texture2D> valid = new List<Texture2D>();
        int width = -1, height = -1;

        ParseTexturesForValid(ref valid, ref width, ref height);

        validated = valid.ToArray();

        int count = valid.Count;
        rend.sharedMaterial.SetInt(textureArrayCountPropertyId, count);

        if (count == 0)
        {
            array = null;
            return;
        }
        
        array = new Texture2DArray(width, height, count, TextureFormat.RGBA32, true, false);
        array.filterMode = filterMode;
        array.wrapMode = wrapMode;

        for(int i = 0; i < valid.Count; i++)
        {
            array.SetPixels(valid[i].GetPixels(0, 0, width, height), i, 0);
        }
        array.Apply();

        rend.sharedMaterial.SetTexture(texturePropertyId, array);
    }

    void ParseTexturesForValid(ref List<Texture2D> valid, ref int width, ref int height)
    {
        if (hardLimitOnSize)
        {
            width = -1;
            height = -1;

            foreach (Texture2D tex in textures)
            {
                var flag = false;

                if (tex != null)
                {
                    var w = tex.width;
                    var h = tex.height;

                    if (width == -1 && height == -1)
                    {
                        m_width = width = w; m_height = height = h;
                        flag = true;
                    }
                    else
                    {
                        if (w == width && h == height)
                            flag = true;
                    }

                    if (flag)
                        valid.Add(tex);
                }
            }
        }
        else
        {
            int minw = -1;
            int minh = -1;

            foreach (Texture2D tex in textures)
            {
                var flag = false;

                if (tex != null)
                {
                    var w = tex.width;
                    var h = tex.height;

                    if(minw == -1 || w < minw)
                        minw = w;

                    if (minh == -1 || h < minh)
                        minh = h;

                    valid.Add(tex);
                }
            }

            width = minw;
            height = minh;
        }
    }

    void OnDestroy()
    {
        if (array != null)
            Texture2DArray.DestroyImmediate(array); // Clear out previous texture if not null
    }

}
