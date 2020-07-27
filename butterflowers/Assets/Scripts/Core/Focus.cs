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
    public static System.Action<State, State> onUpdateState;

	#endregion

	#region External

	[SerializeField] CameraManager CameraManager;
    [SerializeField] FocusCamera Camera;
    [SerializeField] Wizard.Controller Wizard;

    [SerializeField] AudioHandler BackgroundAudio;
    [SerializeField] AudioHandler MemoryAudio;

    #endregion

    #region Internal

    public enum State 
    {
        None,
        Environment,
        Wizard
    }

	#endregion

	#region Properties

	[SerializeField] FocalPoint m_focus = null;
    [SerializeField] FocalPoint focusInQueue = null;

    [SerializeField] FocalPoint[] focalPoints;
    [SerializeField] CameraVisualBlend CameraBlend;

    [SerializeField] ToggleOpacity loading;
    [SerializeField] ui.Image loadingFill;
    [SerializeField] ui.Image tooltip;

    [SerializeField] TrajectoryLine trajectory;
    [SerializeField] Transform trajectory_root;

    #endregion

    #region Attributes

    [SerializeField] State m_state = State.None;

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
    [SerializeField] float focusDelay = 0f;
                     bool delay = false;

    [SerializeField] CameraVisualBlendDefinition[] loseFocusBlends;

	#endregion

	#region Accessors

    public bool active {
        get
        {
            return (m_state != State.None);
        }
    }

    public State state => m_state;

    public FocalPoint focus => m_focus;

    public bool focusing => focusInQueue != null && focus_ready && t_focus-focusDelay < timeToFocus;
    public bool losing_focus => m_focus != null && focus_ready && t_losefocus-focusDelay < timeToLoseFocus;

    #endregion

    #region Monobehaviour callbacks

    void OnEnable() {
        FocalPoint.FocusOnPoint += SetFocus;
        FocalPoint.BeginFocus += NewFocus;

        FocalPoint.HoverFocus += QueueFocus;
        FocalPoint.UnhoverFocus += UnqueueFocus;
    }

    void OnDisable()
    {
        FocalPoint.FocusOnPoint -= SetFocus;
        FocalPoint.BeginFocus -= NewFocus;

        FocalPoint.HoverFocus -= QueueFocus;
        FocalPoint.UnhoverFocus -= UnqueueFocus;
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
            delay = true;
        }
        else {
            if (active)
                delay = (t_losefocus < focusDelay);
            else
                delay = (t_focus < focusDelay);
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
        UpdateTrajectory();


        EvaluateState();


        if (BackgroundAudio != null) {
            if (active) 
            {
                float dist = (m_state == State.Wizard) ? 0f : -1f;

                SetBackgroundAudioFromDistance(dist);
                SetBackgroundVolumeFromDistance(dist);
            }
            else 
            {
                BackgroundAudio.pitch = 1f;
                BackgroundAudio.volume = 1f;

                lowpass = maxBGLP;
            }

            SmoothLowPassFilter();
        }
    }

    #endregion

    #region State

    void EvaluateState()
    {
        State state = State.None;

        if (Wizard != null && Wizard.isFocused) 
        {
            state = State.Wizard;
        }
        else 
        {
            if (focus == null)
                state = State.None;
            else
                state = State.Environment;
        }

        if (state != this.state) 
        {
            m_state = state;
            if (onUpdateState != null)
                onUpdateState(m_state, state);
        }

        
    }

	#endregion

	#region Focusing

	void QueueFocus(FocalPoint focus)
    {
        return;

        var icon = (this.focus != focus)? focus.focusIcon:null;

        tooltip.enabled = (icon != null);
        tooltip.sprite = icon;
    }

    void UnqueueFocus(FocalPoint focus)
    {
        return;

        tooltip.enabled = false;
        tooltip.sprite = null;
    }

    void SettingFocus()
    {
        if (focusInQueue == null) return;

        float t = (t_focus - focusDelay);

        if (t >= timeToFocus) {
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
        if (m_focus == null) return;

        float t = (t_losefocus - focusDelay);

        if (t >= timeToLoseFocus) {
            LoseFocus();

            focus_ready = false;
            t_losefocus = 0f;
        }
        else
            t_losefocus += Time.deltaTime;
    }

    void UpdateLoadingBar()
    {
        if ((focusing || losing_focus) && !delay) {
            loading.transform.position = (focusing)? focusInQueue.screen_anchor : Input.mousePosition;

            loading.Show();

            var len = (focusing) ? t_focus-focusDelay : t_losefocus-focusDelay;
            var dur = (focusing) ? timeToFocus : timeToLoseFocus;

            loadingFill.fillAmount = Mathf.Clamp01(len / dur);
        }
        else {

            loading.Hide();
            loadingFill.fillAmount = 0f;
        }
    }

    void UpdateTrajectory()
    {
        if (focusing && !delay) 
        {
            trajectory.Line.enabled = true;

            float t = Mathf.Clamp01((t_focus - focusDelay) / timeToFocus);
            trajectory.DrawArc(trajectory_root.position, focusInQueue.anchor, t);
        }
        else
            trajectory.Line.enabled = false;
    }

	#endregion

	#region Operations

	public void SetFocus(FocalPoint focus)
    {
        if (focus == this.m_focus) return;

        Dispose();
        this.m_focus = focus;

        Camera = focus.camera;
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
        timeToFocus = focus.timetofocus;
    }

    public void LoseFocus()
    {
        Dispose();
        m_focus = null;


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

            float dist = 0f;

            if(m_state == State.Environment)
                dist = Vector3.Distance(m_focus.transform.position, Camera.transform.position);

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

	void Dispose()
    {
        if (m_focus != null) m_focus.LoseFocus(); // Clear default focus
    }

    void SetBackgroundAudioFromDistance(float distance = -1f)
    {
        if (BackgroundAudio == null || Camera == null) return;

        if(distance < 0f)
            distance = Vector3.Distance(m_focus.transform.position, Camera.transform.position);

        float pitch = distance.RemapNRB(minFocusDistance, maxFocusDistance, minBGPitch, maxBGPitch);
        BackgroundAudio.pitch = pitch;

        lowpass = distance.RemapNRB(minFocusDistance, maxFocusDistance, minBGLP, maxBGLP);
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

    void SetBackgroundVolumeFromDistance(float distance = -1f)
    {
        if (BackgroundAudio == null || Camera == null) return;

        if(distance < 0f)
            distance = Vector3.Distance(m_focus.transform.position, Camera.transform.position);

        float vol = distance.RemapNRB(minFocusDistance, maxFocusDistance, minBGVol, maxBGVol);
        BackgroundAudio.volume = vol;
    }

    #endregion
}
