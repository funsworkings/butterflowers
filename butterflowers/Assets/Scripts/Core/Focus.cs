using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Focus : MonoBehaviour
{

    #region External

    [SerializeField] CameraManager CameraManager;
    [SerializeField] FocusCamera Camera;

    [SerializeField] AudioHandler BackgroundAudio;

    #endregion

    #region Properties

    [SerializeField] FocalPoint focus = null;

    #endregion

    #region Attributes

    [SerializeField] float minFocusDistance = 1f, maxFocusDistance = 10f;

    [SerializeField] float minBGPitch = .4f, maxBGPitch = 1f;
    [SerializeField] float minBGLP = 300f, maxBGLP = 5000f;

    [SerializeField] float lowpass = 0f, lowPassSmoothSpeed = .1f;
    [SerializeField] string lowPassFilterParam = null;

	#endregion

	#region Accessors

    public bool active {
        get
        {
            return (focus != null);
        }
    }

    #endregion

    #region Monobehaviour callbacks

    void OnEnable() {
        FocalPoint.FocusOnPoint += SetFocus;
    }

    void OnDisable()
    {
        FocalPoint.FocusOnPoint -= SetFocus;
    }

    // Update is called once per frame
    void Update()
    {
        if (active) {
            if (Input.GetKeyDown(KeyCode.Escape)) LoseFocus();
        }

        if (BackgroundAudio != null) {
            if (active)
                SetBackgroundAudioFromDistance();
            else {
                BackgroundAudio.pitch = 1f;
                lowpass = maxBGLP;
            }

            SmoothLowPassFilter();
        }
    }

	#endregion

	#region Operations

	public void SetFocus(FocalPoint focus)
    {
        if (focus == this.focus) return;

        Dispose();
        this.focus = focus;

        var Camera = PullCamera();
        if (Camera == null) return;

        Camera.Focus(focus.transform);
        CameraManager.SetCamera(Camera);
    }

    public void LoseFocus()
    {
        Dispose();
        focus = null;

        CameraManager.ResetToDefault();
    }

    #endregion

    #region Internal

    FocusCamera PullCamera()
    {
        return (focus == null || focus.camera == null) ? this.Camera : focus.camera;
    }

	void Dispose()
    {
        if (focus != null) focus.LoseFocus(); // Clear default focus
    }

    void SetBackgroundAudioFromDistance()
    {
        if (BackgroundAudio == null || Camera == null) return;

        float dist = Vector3.Distance(focus.transform.position, Camera.transform.position);

        float pitch = dist.RemapNRB(minFocusDistance, maxFocusDistance, minBGPitch, maxBGPitch);
        BackgroundAudio.pitch = pitch;

        lowpass = dist.RemapNRB(minFocusDistance, maxFocusDistance, minBGLP, maxBGLP);
    }

    void SmoothLowPassFilter()
    {
        var mixer = BackgroundAudio.Mixer;
        if (mixer == null) return;

        float current = 0f;
        mixer.GetFloat(lowPassFilterParam, out current);

        float target = Mathf.Lerp(current, lowpass, Time.deltaTime * lowPassSmoothSpeed);
        mixer.SetFloat(lowPassFilterParam, target);
    }

    #endregion
}
