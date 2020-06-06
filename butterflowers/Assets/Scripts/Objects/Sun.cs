using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System.Linq;
using Settings;

public class Sun : MonoBehaviour
{
    public static Sun Instance;

	#region Events

	public static System.Action onDayBegin, onDayEnd, onNightBegin, onNightEnd; // Time of day events
    public static System.Action onCycle, onStateChange; // Day event

	#endregion

	#region External

	GameDataSaveSystem Save = null;
    [SerializeField] Settings.WorldPreset Preset;

    IReactToSun[] Listeners;
    IObserveSunCycle[] Observers;

    #endregion

    #region Internal

    public enum State { Day, Night }

	#endregion

	#region Properties

	new Light light;

    #endregion

    #region Attributes

    bool setTransform = false;

    Vector3 origin = Vector3.zero;
    Vector3 axis = Vector3.right;
    Vector3 ray = Vector3.zero;

    [SerializeField] float m_time = 0f;
    [SerializeField] float m_timeOfDay = 0f;
    [SerializeField] int m_previousDays = -1, m_days = 0;
    [SerializeField] State state = State.Day;

    public bool active = true;

	#endregion

	#region Accessors

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

    public float speed {
        get
        {
            return Preset.hoursPerDay / Preset.sunHoursPerDay;
        }
    }

	#endregion

	#region Monobehaviour callbacks

	void Awake()
    {
        Instance = this;
        Save = GameDataSaveSystem.Instance;

        light = GetComponent<Light>();
    }

    IEnumerator Start()
    {
        Listeners = FindObjectsOfType<MonoBehaviour>().OfType<IReactToSun>().ToArray();
        Observers = FindObjectsOfType<MonoBehaviour>().OfType<IObserveSunCycle>().ToArray();

        active = false;
        while(!Save.load)
            yield return null;

        time = (Preset.reset)? 0f:Save.time;
        onLoadSunData();
    }

    // Update is called once per frame
    void Update()
    {
        if (!active)
        {
            if (Input.GetKeyDown(KeyCode.Space)) // Override active state (debug)
                active = true;
            return;
        }

        time += Time.deltaTime; // Add time to global clock
        Save.time = time;

        AdjustMeasurements();

        bool statechange = EvaluateTimeState();
        bool advanced = EvaluateDay();

        AlignWithRay();
        UpdateListeners();

        //if (advanced)
          //  active = false; // Pause sun when crosses into new day (debug)
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
        var sundays = Preset.ConvertToDays(time * speed);
        var precisedays = Preset.ConvertToDays(time);

        m_timeOfDay = sundays - Mathf.Floor(sundays);

        m_previousDays = m_days;
        m_days = Mathf.FloorToInt(precisedays);
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
                    if (onNightBegin != null)
                        onNightBegin();
                }
                else { // Night --> Day
                    if (onNightEnd != null)
                        onNightEnd();
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
        if (days > previousDays) {
            Debug.LogFormat("Sun advanced!  from:{0} to:{1}", days-1, days);

            if (onCycle != null)
                onCycle();

            return true;
        }
        return false;
    }

    void UpdateListeners()
    {
        foreach (IReactToSun listener in Listeners) {
            listener.ReactToTimeOfDay(timeOfDay);
            listener.ReactToDay(days);
        }
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
