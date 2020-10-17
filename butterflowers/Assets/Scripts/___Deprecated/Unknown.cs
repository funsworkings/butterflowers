using Settings;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Profiling;
using uwu.Extensions;

public class Unknown : MonoBehaviour
{

    #region External

    [SerializeField] ButterflowerManager Butterflies;
    [SerializeField] Nest Nest;
    [SerializeField] World World;

    #endregion

    #region Internal

    public enum Pattern { None, River, Perlin, Feed }

    #endregion

    #region Properties

    [SerializeField] WorldPreset Preset;
    [SerializeField] Camera feedCamera;

	#endregion

	#region Attributes

	[Header("Global attributes")]
	[SerializeField] float scribeInterval = 1f;
    [SerializeField] int totalwidth = 32;
    [SerializeField] Pattern patt = Pattern.None;
    [SerializeField] bool events = true;


    [Header("River")]
    [SerializeField] int riverposition = 0;
    [SerializeField] int riverwidth = 8, minriverwidth = 2, maxriverwidth = 16;
    [SerializeField] float minriverwavelength = 0f, maxriverwavelength = 3.33f;
    [SerializeField] float riverspeed = 1f;

    [SerializeField] char riverchar = '~';
    [SerializeField] char normalchar = '0';

    [Header("Perlin")]
    [SerializeField] Vector2 noiseSize = Vector2.one;
    [SerializeField] char[] noise_chars = new char[] { };

    [Header("Feed")]
    [SerializeField] RenderTexture feedTexture;
    [SerializeField] Texture2D sampler;
    [SerializeField] char wiz_char = '+';
    [SerializeField] int _y = 0;
    [SerializeField] float feedSpeed = 1f;

    #endregion

    #region Internal

    public delegate string patternFunction();

	#endregion

	bool plague = false;

    void OnEnable()
    {
        if(events)
            Events.onFireEvent += onFireEvent;
        //feedCamera.enabled = false;
    }

    void OnDisable()
    {
        if(events)
            Events.onFireEvent -= onFireEvent;
    }

    void onFireEvent(EVENTCODE @event, AGENT a, AGENT b, string details)
    {
        if (@event != EVENTCODE.NESTGROW) return;
/*
        if (World.FetchKnowledgeOfWizard() < 1f) 
        {
            int level = Nest.LEVEL;

            // Evaluate PATTERN
            if (level == 0)
                patt = Pattern.None;
            else if (level == 1)
                patt = Pattern.Perlin;
            else if (level == 2)
                patt = Pattern.River;
            else
                patt = Pattern.Feed;
        }
        else
            patt = Pattern.Feed;*/

        StartCoroutine("Plague");
    }

    void OnDestroy()
    {
        Texture2D.Destroy(sampler);
    }

    IEnumerator Plague()
    {
        bool healthy = false;

        float health = Butterflies.GetHealth();
        while (!healthy || health > Preset.unknownPersistenceThreshold) 
        {
            var patt = fetchPattern();
            var @string = patt();

            //EventManager.Instance.Push(EVENTCODE.UNKNOWN, AGENT.Unknown, AGENT.World, @string, false);
            yield return new WaitForSeconds(Mathf.Max(0f, scribeInterval));

            health = Butterflies.GetHealth();
            if (!healthy && health > Preset.unknownPersistenceThreshold)
                healthy = true;
        }
    }

    patternFunction fetchPattern()
    {
        patternFunction a = none;

        //feedCamera.enabled = (patt == Pattern.Feed);

        if (patt == Pattern.River)
            a = river;
        else if (patt == Pattern.Perlin)
            a = perlin;
        else if (patt == Pattern.Feed)
            a = feed;

        return a;
    }

    #region None

    public string none()
    {
        var row = new string(normalchar, totalwidth);
        return row;
    }

	#endregion

	#region River

	public string river()
    {
        float health = (Butterflies != null)? Butterflies.GetHealth():1f;

        float wavelength = health.RemapNRB(0f, 1f, minriverwavelength, maxriverwavelength);
        float slope = -(wavelength * Mathf.Sin(Time.time * riverspeed));

        riverposition += Mathf.RoundToInt(slope);

        float size = Random.Range(-1f, 1f);
        riverwidth = Mathf.Clamp(riverwidth + Mathf.RoundToInt(size), minriverwidth, maxriverwidth);

        /* * * * * * * * * * * * * * * * */

        var row = new string(normalchar, totalwidth);

        int start = Mathf.Clamp(riverposition - riverwidth/2, 0, totalwidth-1);
        int end = Mathf.Clamp(riverposition + riverwidth / 2, 0, totalwidth-1);

        if (start == end) return row;

        var chars = row.ToCharArray();
        for (int i = start; i <= end; i++)
            chars[i] = riverchar;

        return new string(chars);
    }

    #endregion

    #region Perlin

    public string perlin()
    {
        var row = new string(normalchar, totalwidth);
        var chars = row.ToCharArray();
        var len = noise_chars.Length-1;
        float t = Time.time;

        if (noise_chars.Length > 0) 
        {
            for (int i = 0; i < totalwidth; i++) {
                float perl = Mathf.PerlinNoise((i+t) * noiseSize.x, (i+t) * noiseSize.y);
                float ramp = perl * len;
                int round = Mathf.RoundToInt(ramp);

                chars[i] = noise_chars[round];
            }
        }

        return new string(chars);
    }

    #endregion

    #region Feed

    public string feed()
    {
        var row = new string(normalchar, totalwidth);
        var chars = row.ToCharArray();


        var render = RenderTexture.active;

        var w = feedTexture.width;
        var h = feedTexture.height;
        var x = 0;

        RenderTexture.active = feedTexture;

        if (sampler == null){
            sampler = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
            sampler.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
            sampler.Apply();
        }

        if (_y == 0) 
        {
            sampler.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
            sampler.Apply();
        }

        var sample = sampler.GetPixels();
        RenderTexture.active = render;

        float xi = (float)w / chars.Length;
        for (int i = 0; i < chars.Length; i++) 
        {
            x = (w * _y);

            var _i = (sample.Length-1) - (Mathf.FloorToInt((chars.Length - i)*xi) + x);
            if (sample[_i].a > 0f)
                chars[i] = wiz_char;
        }

        float uh = chars.Length * ((float)h / w);
        float hi = (float)h / uh;

        _y += ((int)(hi * feedSpeed));
        if (_y >= h)
            _y = 0;

        return new string(chars);
    }

	#endregion
}
