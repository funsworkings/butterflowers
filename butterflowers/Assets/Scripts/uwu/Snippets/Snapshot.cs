using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

[RequireComponent(typeof(Camera))]
public class Snapshot : MonoBehaviour
{
    public System.Action<Texture2D> onSuccess;
    public UnityEvent onStart, onEnd;

    Camera m_camera;
    bool inprogress = false;

    public new Camera camera => m_camera;

    void Awake()
    {
        m_camera = GetComponent<Camera>();
    }

    public void Capture()
    {
        if (!inprogress) 
        {
            StartCoroutine("Capturing");
            inprogress = true;
        }
    }

    IEnumerator Capturing()
    {
        yield return new WaitForEndOfFrame();

        var w = Screen.width;
        var h = Screen.height;

        var screenshot = new Texture2D(w, h, TextureFormat.RGB24, false);

        var rt = new RenderTexture(w, h, 24);

        m_camera.targetTexture = rt;
        m_camera.Render();

        var prt = RenderTexture.active;
        RenderTexture.active = rt;

        screenshot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        screenshot.Apply();

        m_camera.targetTexture = null;
        RenderTexture.active = prt;

        Destroy(rt);

        onStart.Invoke();
        yield return new WaitForEndOfFrame();
        onEnd.Invoke();

        inprogress = false;
        if (onSuccess != null)
            onSuccess(screenshot);
    }
}
