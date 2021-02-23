using System;
using System.Linq;
using butterflowersOS.Core;
using butterflowersOS.Data;
using butterflowersOS.Interfaces;
using butterflowersOS.Presets;
using TMPro;
using UnityEngine;
using uwu.Timeline.Core;

namespace butterflowersOS.Objects.Entities
{
    public class Sun : MonoBehaviour
    {
        public static Sun Instance;

        // Events

        public static System.Action onDayBegin, onDayEnd, onNightBegin, onNightEnd; // Time of day events
        public static System.Action onCycle, onStateChange; // Day event

        // External
    
        [SerializeField] WorldPreset Preset = null;
        [SerializeField] Cutscenes Cutscenes = null;

        World World;
    
        IReactToSun[] Listeners = new IReactToSun[]{};
        IReactToSunCycle[] Observers = new IReactToSunCycle[]{};
        IPauseSun[] Pausers = new IPauseSun[]{};

        #region Internal

        public enum State { Day, Night }

        #endregion

        // Properties

        new Light light;

        [SerializeField] Animator dayTracker = null;
        [SerializeField] TMP_Text previousDayText = null, currentDayText = null;
    
        // Attributes

        bool setTransform = false;

        Vector3 origin = Vector3.zero;
        Vector3 axis = Vector3.right;
        Vector3 ray = Vector3.zero;

        [SerializeField] float m_time = 0f;
        [SerializeField] float m_timeOfDay = 0f;
        [SerializeField] float m_progress = 0f;
        [SerializeField] int m_previousDays = -1, m_days = 0;
        [SerializeField] State state = State.Day;

        [SerializeField] float minTimeScale = .001f, maxTimeScale = 1f, timeScale = 1f;
        [SerializeField] float timeScaleLerpSpeed = 1f;
    
        public bool active = true;

        #region Accessors
    
        public float intensity
        {
            get { return light.intensity; }
            set { light.intensity = value; }
        }

        public float time
        {
            get
            {
                return m_time;
            }
            set
            {
                m_time = Mathf.Max(0f, value);
                AdjustRayFromTime();
            }
        }

        public int days
        {
            get
            {
                return m_days;
            }
        }

        public int previousDays {
            get
            {
                return m_previousDays;
            }
        }

        public float timeOfDay
        {
            get
            {
                return m_timeOfDay;
            }
        }

        public float interval
        {
            get
            {
                return m_progress;
            }
        }

        public float speed {
            get
            {
                return Preset.hoursPerDay / Preset.sunHoursPerDay;
            }
        }

        public float progress => m_progress;

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            Instance = this;

            light = GetComponent<Light>();

            active = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (World == null) return;
            if (!World.LOAD) return;
        
            float t_timeScale = 0f;
            if (!active) 
            {
                WaitForPausers();
                t_timeScale = minTimeScale;
            }
            else 
            {
                t_timeScale = maxTimeScale;
            }

            if (Cutscenes.playing) t_timeScale = maxTimeScale; // Override time scale if cutscenes in progress!
            

            timeScale = Mathf.Lerp(timeScale, t_timeScale, Time.unscaledDeltaTime * timeScaleLerpSpeed);
            Time.timeScale = World.TimeScale * timeScale;

            if (active) 
            {
                if (Input.GetKeyDown(KeyCode.RightBracket) && Preset.allowDebugTimeSkip) time += Preset.secondsPerDay;

                time += Time.deltaTime; // Add time to global clock

                AdjustMeasurements();

                bool statechange = EvaluateTimeState();
                bool advanced = EvaluateDay();

                AlignWithRay();
                UpdateListeners();

                //if (advanced)
                //  active = false; // Pause sun when crosses into new day (debug)
            }
        }

        void OnDestroy()
        {
            Time.timeScale = 1f; // Reset time scale back to normal
        }

        #endregion
    
        #region Save/load

        public object Save()
        {
            var dat = new SunData();
            dat.active = active;
            dat.time = time;

            return dat;
        }

        public void Load(object data)
        {
            World = World.Instance;
            Listeners = FindObjectsOfType<MonoBehaviour>().OfType<IReactToSun>().ToArray();
            Observers = FindObjectsOfType<MonoBehaviour>().OfType<IReactToSunCycle>().ToArray();
            Pausers = FindObjectsOfType<MonoBehaviour>().OfType<IPauseSun>().ToArray();

            SunData dat = (SunData) data;
            active = dat.active;
            time = dat.time;
            
            WaitForPausers();
            onLoadSunData();
            InitializeUI();
        }
    
        #endregion

        #region Internal

        void AlignWithRay()
        {
            var bearing = transform.forward;
            var directionBetween = (ray - transform.forward);

            //transform.forward += (directionBetween * Mathf.Min(1f, Time.realtimeSinceStartup / days)); **slow sun down over time
            transform.forward += directionBetween;
        }

        void AdjustRayFromTime() {
            float interval = Mathf.Repeat(time / Preset.secondsPerDay * speed, 1f);
            float angle = Mathf.Repeat(interval-.25f, 1f) * 360f;

            ray = Quaternion.AngleAxis(angle, axis) * origin;
        }

        void AdjustMeasurements()
        {
            var sundays = Preset.ConvertSecondsToDays(time * speed);
            var precisedays = Preset.ConvertSecondsToDays(time);

            m_timeOfDay = sundays - Mathf.Floor(sundays);

            m_previousDays = m_days;
            m_days = Mathf.FloorToInt(precisedays);
        
            m_progress = 1f -(Preset.ConvertDaysToSeconds(m_days + 1) - time) /
                Preset.secondsPerDay;
        }

        bool EvaluateTimeState(bool events = true)
        {
            State previousState = state;

            if (Preset.IsDuringDay(timeOfDay))
                state = State.Day;
            else
                state = State.Night;

            if (previousState != state) {
                if (events) {
                    if (previousState == State.Day) { // Day --> Night

                        if (onDayEnd != null)
                            onDayEnd();

                        Events.ReceiveEvent(EVENTCODE.NIGHT, AGENT.World, AGENT.Terrain);
                        if (onNightBegin != null)
                            onNightBegin();
                    }
                    else { // Night --> Day

                        if (onNightEnd != null)
                            onNightEnd();

                        Events.ReceiveEvent(EVENTCODE.DAY, AGENT.World, AGENT.Terrain);
                        if (onDayBegin != null)
                            onDayBegin();
                    }
                }

                if (onStateChange != null)
                    onStateChange();

                //Debug.LogFormat("Sun state moved from {0} --> {1}", previousState, state);
                return true;
            }
            return false;
        }

        bool EvaluateDay()
        {
            if (days > previousDays) 
            {
                Debug.LogFormat("Sun advanced!  from:{0} to:{1}", days-1, days);

                Events.ReceiveEvent(EVENTCODE.CYCLE, AGENT.World, AGENT.Terrain, days + "");

                CycleObservers(true);

                Pausers = FindObjectsOfType<MonoBehaviour>().OfType<IPauseSun>().ToArray();
                WaitForPausers();
                AdvanceUI(days-1, days);
            
                if (onCycle != null)
                    onCycle();

                return true;
            }
            return false;
        }

        #endregion
    
        #region UI

        void InitializeUI()
        {
            currentDayText.text = days.ToString();
        }

        void AdvanceUI(int previous, int current)
        {
            previousDayText.text = (previous).ToString();
            currentDayText.text = (current).ToString();
        
            dayTracker.SetTrigger("advance");
        }
    
        #endregion
    
        #region Listeners and observers
    
        void UpdateListeners()
        {
            foreach (IReactToSun listener in Listeners) {
                listener.ReactToTimeOfDay(timeOfDay);
                listener.ReactToDay(days);
            }
        }

        void CycleObservers(bool refresh)
        {
            Observers = FindObjectsOfType<MonoBehaviour>().OfType<IReactToSunCycle>().ToArray();

            bool includeAll = (World.type == World.AdvanceType.Broken);
            bool success = false;
            
            foreach (IReactToSunCycle o in Observers) 
            {
                success = (includeAll || (o is IReactToSunCycleReliable));
                if(success) o.Cycle(refresh);
            }
        }

        public void WaitForPausers() // Trigger pause cycle if required!
        {
            bool active = true;
        
            foreach (IPauseSun pauser in Pausers) 
            {
                if (pauser != null && pauser.Pause) 
                {
                    active = false;
                    break;
                }    
            }

            this.active = active;
        }
    
        #endregion

        #region Internal callbacks

        void onLoadSunData(){
            if (!setTransform)
            {
                origin = transform.forward;
                axis = transform.right;
            }

            AdjustMeasurements();
            EvaluateTimeState(false);
        }

        #endregion
    }
}
