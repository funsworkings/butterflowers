using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UIExt.Behaviors.Visibility;
using UnityEngine;

using ui = UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Focus : MonoBehaviour
{

    #region Events

    public UnityEvent onFocus, onLoseFocus;

	#endregion

	#region External

	[SerializeField] CameraManager CameraManager;
    [SerializeField] FocusCamera Camera;

    [SerializeField] AudioHandler BackgroundAudio;
    [SerializeField] AudioHandler MemoryAudio;

    #endregion

    #region Properties

    [SerializeField] FocalPoint focus = null;
    [SerializeField] FocalPoint focusInQueue = null;

    [SerializeField] FocalPoint[] focalPoints;
    [SerializeField] CameraVisualBlend CameraBlend;

    [SerializeField] ToggleOpacity loading;
    [SerializeField] ui.Image loadingFill;

    #endregion

    #region Attributes

    [SerializeField] float minFocusDistance = 1f, maxFocusDistance = 10f;

    [SerializeField] float minBGPitch = .4f, maxBGPitch = 1f;
    [SerializeField] float minBGLP = 300f, maxBGLP = 5000f;
    [SerializeField] float minBGVol = 0f, maxBGVol = 1f;

    [SerializeField] float minMemVol = 0f, maxMemVol = 1f;

    [SerializeField] float lowpass = 0f, lowPassSmoothSpeed = .1f;
    [SerializeField] string lowPassFilterParam = null;

    [SerializeField] float timeToFocus, timeToLoseFocus = 1f;
                     float t_focus = 0f, t_losefocus = 0f;
                     bool focus_ready = false;

    [SerializeField] CameraVisualBlendDefinition[] loseFocusBlends;

	#endregion

	#region Accessors

    public bool active {
        get
        {
            return (focus != null);
        }
    }

    public bool focusing => focusInQueue != null && focus_ready && t_focus < timeToFocus;
    public bool losing_focus => focus != null && focus_ready && t_losefocus < timeToLoseFocus;

    #endregion

    #region Monobehaviour callbacks

    void OnEnable() {
        FocalPoint.FocusOnPoint += SetFocus;
        FocalPoint.BeginFocus += NewFocus;
    }

    void OnDisable()
    {
        FocalPoint.FocusOnPoint -= SetFocus;
        FocalPoint.BeginFocus -= NewFocus;
    }

    void Start()
    {
        focalPoints = FindObjectsOfType<FocalPoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) focus_ready = true;
        if (Input.GetMouseButtonUp(0)) focus_ready = false;

        if (!focus_ready) 
        {
            t_focus = t_losefocus = 0f;
        }

        if (active) 
        {
            if (focus_ready) {
                if (focusInQueue == null)
                    LosingFocus();
                else
                    SettingFocus();
            }
        }
        else 
        {
            if (focus_ready) 
            {
                if (focusInQueue != null)
                    SettingFocus();
            }
            else
                focusInQueue = null;
        }
        UpdateLoadingBar();

        if (BackgroundAudio != null) {
            if (active) {
                SetBackgroundAudioFromDistance();
                SetBackgroundVolumeFromDistance();
            }
            else {
                BackgroundAudio.pitch = 1f;
                BackgroundAudio.volume = 1f;

                lowpass = maxBGLP;
            }

            SmoothLowPassFilter();
        }
    }

    #endregion

    #region Focusing

    void SettingFocus()
    {
        if (focusInQueue == null) return;

        if (t_focus >= timeToFocus) {
            focusInQueue.Focus();

            focusInQueue = null;
            focus_ready = false;
            t_focus = 0f;
        }
        else {
            t_focus += Time.deltaTime;
        }
    }

    void LosingFocus()
    {
        if (focus == null) return;

        if (t_losefocus >= timeToLoseFocus) {
            LoseFocus();

            focus_ready = false;
            t_losefocus = 0f;
        }
        else
            t_losefocus += Time.deltaTime;
    }

    void UpdateLoadingBar()
    {
        if (focusing || losing_focus) {
            loading.Show();

            var len = (focusing) ? t_focus : t_losefocus;
            var dur = (focusing) ? timeToFocus : timeToLoseFocus;

            loadingFill.fillAmount = Mathf.Clamp01(len / dur);
        }
        else {
            loading.Hide();

            loadingFill.fillAmount = 0f;
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

        CameraBlend.blendDefinition = null;
        CameraBlend.BlendTo(Camera);

        onFocus.Invoke();
    }

    void NewFocus(FocalPoint focus)
    {
        focusInQueue = focus;
        t_focus = 0f;
    }

    public void LoseFocus()
    {
        Dispose();
        focus = null;


        var loseFocusBlend = loseFocusBlends.PickRandomSubset(1)[0];

        CameraBlend.blendDefinition = loseFocusBlend;
        CameraBlend.BlendTo(CameraManager.DefaultCamera);

        onLoseFocus.Invoke();
    }

    #endregion

    #region Audio

    public void SetMemoryVolumeWithDistance(float baseline)
    {
        if (MemoryAudio == null || Camera == null) return;

        if (active) {

            float dist = Vector3.Distance(focus.transform.position, Camera.transform.position);
            float vol = dist.RemapNRB(minFocusDistance, maxFocusDistance, maxMemVol, minMemVol);
            vol = Mathf.Max(minMemVol, vol * baseline);

            MemoryAudio.volume = vol;
        }
        else {
            MemoryAudio.volume = minMemVol;
        }
        
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

    void SetBackgroundVolumeFromDistance()
    {
        if (BackgroundAudio == null || Camera == null) return;

        float dist = Vector3.Distance(focus.transform.position, Camera.transform.position);

        float vol = dist.RemapNRB(minFocusDistance, maxFocusDistance, minBGVol, maxBGVol);
        BackgroundAudio.volume = vol;
    }

    #endregion
}
