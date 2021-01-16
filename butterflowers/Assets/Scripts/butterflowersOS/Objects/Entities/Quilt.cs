using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Core;
using butterflowersOS.Presets;
using UnityEngine;
using uwu.Extensions;
using uwu.Snippets;
using uwu.Textures;

namespace butterflowersOS.Objects.Entities
{
    public class Quilt : MonoBehaviour, ITextureReceiver
    {
        public static Quilt Instance = null;
    
        // External
    
        Library Lib;

        // Properties

        [SerializeField] WorldPreset preset;
    
        [SerializeField] Material material = null;
        [SerializeField] CreateTextureArray textureArray;
        [SerializeField] RenderTexture canvas;
    
        // Collections
    
        [SerializeField] List<Texture2D> textures = new List<Texture2D>();
        List<Texture2D> overridetextures = new List<Texture2D>();
    
        public int textureCap = 4;
    
        // Attributes
    
        [SerializeField] float m_speed = 0f;
        [SerializeField] float minLerpSpeed = 0f, maxLerpSpeed = 1f;
        [SerializeField] float offset = 0f;
        [SerializeField] float interval = 0f;
        [SerializeField] float m_death = 0f, m_deathDecay = 1f;
    
        [SerializeField] float refreshRate = .167f;
    
        // Private fields

        int index = -1;

        Texture2D sampler;
        Color[] sample;
        int w = 0, h = 0;

        #region Accessors
    
        Texture2D[] textureStack
        {
            get
            {
                if (overridetextures.Count > 0)
                    return overridetextures.ToArray();

                return textures.ToArray();
            }
        }
    
        public float speedInterval {
            get
            {
                interval = (m_speed - minLerpSpeed) / (maxLerpSpeed - minLerpSpeed);
                return interval;
            }
        }
    
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
    
        #endregion
    
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

        void Start()
        {
            Lib = Library.Instance;

            textures = new List<Texture2D>();
            overridetextures = new List<Texture2D>();

            textureCap = preset.nestCapacity;

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
            material.SetInt("_TextureRange", textureStack.Length);

            UpdateLerpSpeed();
            if(wait <= 0f)
                offset = Mathf.Repeat(offset + (Time.deltaTime * m_speed), 1f);
        }

        float wait = 0f;
        IEnumerator Wait()
        {
            while (wait > 0f) {
                wait = Mathf.Max(0f, wait - Time.deltaTime);
                yield return null;
            }

        }

        #region Operations

        public void Push(string file)
        {
            Lib.RequestTexture(file, this);
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

        void Add(Texture tex){
            ++index;

            var texture = (tex as Texture2D);

            textures.Add(texture);
            ApplyTextures();
        }

        void Remove(Texture tex) 
        {
            var texture = (tex as Texture2D);

            if (textures.Contains(texture)) {
                textures.Remove(texture);
                --index;
            
                ApplyTextures();
            }

            death = 1f;
        }

        public void Dispose(bool apply = true)
        {
            index = 0;
            textures = new List<Texture2D>();

            if (apply) ApplyTextures();
        }

        #endregion

        #region Sampling

        void SetTextureAttributes()
        {
            w = canvas.width;
            h = canvas.height;

            /*
        sampler = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
        Sample();

        StartCoroutine("Refresh");
        */
        }

        /*
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
    */

        #endregion

        #region Load

        public void ReceiveTexture(string file, Texture2D texture)
        {
            Add(texture);
        }

        #endregion
    
        #region Overrides

        public void PushOverrideTexture(string file)
        {
            var texture = Lib.RequestTextureImmediate(file);
            if (texture != null) 
            {
                overridetextures.Add(texture);
                ApplyTextures();
            }
        }

        public void PopOverrideTexture(string file)
        {
            var texture = Lib.RequestTextureImmediate(file);
            if (texture != null) 
            {
                overridetextures.Remove(texture);
                ApplyTextures();
            }
        }
    
        #endregion

        #region Textures

        void ApplyTextures(){
            if(material == null)
                return;

            int count = textureStack.Length;
        
            List<Texture2D> valid = new List<Texture2D>();
            valid.AddRange(textureStack);
            Texture2D[] append = new Texture2D[(textureCap - count)];
            valid.AddRange(append);
        
            material.SetTexture("_Texture0", valid[0]);
            material.SetTexture("_Texture1", valid[1]);
            material.SetTexture("_Texture2", valid[2]);
            material.SetTexture("_Texture3", valid[3]);
            material.SetTexture("_Texture4", valid[4]);
            material.SetTexture("_Texture5", valid[5]);

            material.SetInt("_TextureCount", count);

            //textureArray.PopulateTextureArray(textureStack);
            UpdateLerpSpeed();

            offset = 1f; // Snap back immediately to last added

            if (wait <= 0f) 
            {
                wait = 1.67f;
                StartCoroutine("Wait");
            }
        }
    
        void UpdateLerpSpeed(){
            float percent = (float)textureStack.Length / textureCap;
            m_speed = percent.Remap(0f, 1f, minLerpSpeed, maxLerpSpeed);

            material.SetFloat("_LerpSpeed", m_speed);
        }

        #endregion
    
    }
}