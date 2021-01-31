using System.Collections.Generic;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using uwu.Audio;
using uwu.Camera;
using uwu.Camera.Instances;
using uwu.Extensions;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Core
{
    public class Focusing : MonoBehaviour
    {
        #region Internal

        public enum State 
        {
            None,
            Environment
        }

        #endregion

        // Events

        public UnityEvent onFocus, onLoseFocus;
        public static System.Action<State, State> onUpdateActiveState;

        // External

        [SerializeField] CameraManager CameraManager;
        [SerializeField] FocusCamera Camera;
        [SerializeField] AudioHandler BackgroundAudio;

        Sun sun;

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
        [SerializeField] AudioMixer mixer;
        [SerializeField] float minBGPitch = .4f, maxBGPitch = 1f;
        [SerializeField] float minBGLP = 300f, maxBGLP = 5000f;
        [SerializeField] float minBGVol = 0f, maxBGVol = 1f;

        [SerializeField] string pitchParam;

        [SerializeField] float lowpass = 0f, lowPassSmoothSpeed = .1f;
        [SerializeField] string lowPassFilterParam = null;
        
        // Collections
    
        Focusable[] focuses = new Focusable[]{};
    
        #region Accessors

        public bool active {
            get
            {
                return (m_state != State.None);
            }
        }

        public State state => m_state;
    
        public Focusable focus => m_focus;
        public GameCamera FallbackCamera => CameraManager.DefaultCamera;

        #endregion

        #region Monobehaviour callbacks

        void OnEnable() 
        {
            Focusable.FocusOnPoint += SetFocus;
        }

        void OnDisable()
        {
            Focusable.FocusOnPoint -= SetFocus;
        }

        void Start()
        {
            focuses = FindObjectsOfType<Focusable>();
            sun = Sun.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            if (active && sun.active) 
            {
                if (Input.GetKeyDown(KeyCode.Backspace))
                    LoseFocus();
            }

            EvaluateState();
            EvaluateAudio();
        }
    
        void Dispose()
        {
            if (m_focus != null) 
                m_focus.LoseFocus(); // Clear default focus
        }

        #endregion

        #region State

        void EvaluateState()
        {
            State state = State.None;

            if (focus == null)
                state = State.None;
            else
                state = State.Environment;

            if (state != this.state) 
            {
                m_state = state;
                if (onUpdateActiveState != null)
                    onUpdateActiveState(m_state, state);
            }
        }

        #endregion

        #region Focus

        public void SetFocus(Focusable focus)
        {
            if (focus == this.m_focus) return;

            if (focus.dispose) 
            {
                Dispose();

                this.m_focus = focus;

                Camera = focus.camera;
                if (Camera != null) 
                {
                    Camera.Focus(focus.transform);

                    CameraBlending.blendDefinition = null;
                    CameraBlending.BlendTo(Camera);
                
                    Events.ReceiveEvent(EVENTCODE.REFOCUS, AGENT.User, focus.Agent);
                }
                else 
                {
                    LoseFocus();    
                }
            }
        
            onFocus.Invoke();
            //overlayOpacity.Show();
        }

        public void LoseFocus()
        {
            Dispose();
            m_focus = null;
        
            var loseFocusBlend = loseFocusBlends.PickRandomSubset(1)[0];

            CameraBlending.blendDefinition = loseFocusBlend;
            CameraBlending.BlendTo(CameraManager.DefaultCamera);

            onLoseFocus.Invoke();
            //overlayOpacity.Hide();
        }

        public Focusable[] FindVisibleFocuses()
        {
            List<Focusable> focus = new List<Focusable>();
            if (active) 
                focus.Add(null); // Add escape option

            foreach (Focusable f in focuses) 
            {
                if (f != this.focus) 
                {
                    var pos = Vector2.zero;
                    if (f.transform.IsVisible(CameraManager.MainCamera, out pos))
                        focus.Add(f);
                }
            }
        
            return focus.ToArray();
        }

        #endregion

        #region Audio

        void EvaluateAudio()
        {
            if (active) 
            {
                SetBackgroundAudioFromDistance(-1f);
                //SetBackgroundVolumeFromDistance(-1f);
            }
            else 
            {
                mixer.SetFloat(pitchParam, 1f);
                //BackgroundAudio.pitch = 1f;
                //BackgroundAudio.volume = 1f;

                lowpass = maxBGLP;
            }

            SmoothLowPassFilter();
        }

        void SetBackgroundAudioFromDistance(float distance = -1f)
        {
            if (Camera == null) return;

            if(distance < 0f)
                distance = Vector3.Distance(m_focus.transform.position, Camera.transform.position);

            float pitch = distance.RemapNRB(minFocusDistance, maxFocusDistance, minBGPitch, maxBGPitch);
            mixer.SetFloat(pitchParam, pitch);

            lowpass = distance.RemapNRB(minFocusDistance, maxFocusDistance, minBGLP, maxBGLP);
        }

        void SmoothLowPassFilter()
        {
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
}
