using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

using ui = UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using uwu.Audio;
using uwu.Camera;
using uwu.Camera.Instances;
using uwu.Extensions;
using uwu.UI.Behaviors.Visibility;

public class Focusing : MonoBehaviour
{
    #region Internal

    public enum State 
    {
        None,
        Environment,
        Wizard
    }

    #endregion

    // Events

    public UnityEvent onFocus, onLoseFocus;
    public static System.Action<State, State> onUpdateState;

	// External

	[SerializeField] CameraManager CameraManager;
    [SerializeField] FocusCamera Camera;
    [SerializeField] Wizard.Controller Wizard;

    [SerializeField] AudioHandler BackgroundAudio;
    [SerializeField] AudioHandler MemoryAudio;

    // Properties

	[SerializeField] Focusable m_focus = null;
    [SerializeField] CameraVisualBlend CameraBlending;
    [SerializeField] ToggleOpacity overlayOpacity;

    // Attributes

    [SerializeField] State m_state = State.None;

    [SerializeField] float minFocusDistance = 1f, maxFocusDistance = 10f;
    
    [Header("Camera blends")]
        [SerializeField] CameraVisualBlendDefinition[] loseFocusBlends;

    [Header("Audio")]
        [SerializeField] float minBGPitch = .4f, maxBGPitch = 1f;
        [SerializeField] float minBGLP = 300f, maxBGLP = 5000f;
        [SerializeField] float minBGVol = 0f, maxBGVol = 1f;

        [SerializeField] float minMemVol = 0f, maxMemVol = 1f;

        [SerializeField] float lowpass = 0f, lowPassSmoothSpeed = .1f;
        [SerializeField] string lowPassFilterParam = null;

    #region Accessors

    public bool active {
        get
        {
            return (m_state != State.None);
        }
    }

    public State state => m_state;
    
    public Focusable focus => m_focus;

    #endregion

    #region Monobehaviour callbacks

    void OnEnable() {
        Focusable.FocusOnPoint += SetFocus;
    }

    void OnDisable()
    {
        Focusable.FocusOnPoint -= SetFocus;
    }

    // Update is called once per frame
    void Update()
    {
        if (active) {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LoseFocus();
            }
        }

        EvaluateState();
        EvaluateAudio();
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

    #region Focus

	public void SetFocus(Focusable focus)
    {
        if (focus == this.m_focus) return;

        if (focus.dispose) {
            Dispose();

            this.m_focus = focus;

            Camera = focus.camera;
            if (Camera != null) {
                Camera.Focus(focus.transform);

                CameraBlending.blendDefinition = null;
                CameraBlending.BlendTo(Camera);
                
                Events.ReceiveEvent(EVENTCODE.REFOCUS, AGENT.User, focus.Agent);
            }
        }
        
        onFocus.Invoke();
        overlayOpacity.Show();
    }

    public void LoseFocus()
    {
        Dispose();
        m_focus = null;
        
        var loseFocusBlend = loseFocusBlends.PickRandomSubset(1)[0];

        CameraBlending.blendDefinition = loseFocusBlend;
        CameraBlending.BlendTo(CameraManager.DefaultCamera);

        onLoseFocus.Invoke();
        overlayOpacity.Hide();
    }

    #endregion

    #region Audio

    void EvaluateAudio()
    {
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

    #region Internal

	void Dispose()
    {
        if (m_focus != null) 
            m_focus.LoseFocus(); // Clear default focus
    }

    #endregion
}
