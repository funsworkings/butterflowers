using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System.Linq;

public class Sun : MonoBehaviour
{
    public System.Action onCycle;


    public static Sun Instance;
    IObserveSunCycle[] Observers;


    [System.Serializable]
    public class Data
    {
        public float time = 0f;
        public float intensity = 0f;
    }

    [SerializeField] Settings.WorldPreset world;
    new Light light;


    bool setTransform = false;
    Vector3 origin = Vector3.zero;
    Vector3 axis = Vector3.right;

    Vector3 ray = Vector3.zero;

    const float SLEEPTIME = 128f;

    [SerializeField] float m_time = 0f;
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


    int previousDays = -1;

    [SerializeField] float m_days = 0f;
    public int days
    {
        get
        {
            m_days = world.ConvertToDays(time);
            return Mathf.FloorToInt(m_days);
        }
    }

    [SerializeField] float m_timeOfDay = 0f;
    public float timeOfDay
    {
        get
        {
            m_days = world.ConvertToDays(time);
            m_timeOfDay = m_days - Mathf.Floor(m_days);

            return m_timeOfDay;
        }
    }


    public bool reset = false;


    IReactToSun[] listeners;


    void Awake()
    {
        Instance = this;

        light = GetComponent<Light>();
    }

    void Start()
    {
        listeners = FindObjectsOfType<MonoBehaviour>().OfType<IReactToSun>().ToArray();
        Observers = FindObjectsOfType<MonoBehaviour>().OfType<IObserveSunCycle>().ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime; // Add time to global clock

        CheckForNewDay();

        AlignWithRay();
        UpdateListeners();
    }

    void AlignWithRay()
    {
        var bearing = transform.forward;
        var directionBetween = (ray - transform.forward);

        transform.forward += (directionBetween * Mathf.Min(1f, Time.realtimeSinceStartup / days));
    }

    void AdjustRayFromTime() {
        if (world == null)
            return;

        float interval = Mathf.Repeat(time / world.secondsPerDay, 1f);
        float angle = Mathf.Repeat(interval-.25f, 1f) * 360f;

        ray = Quaternion.AngleAxis(angle, axis) * origin;
    }

    void CheckForNewDay()
    {
        var currentDays = days;
        if(currentDays > previousDays)
        {
            Debug.LogFormat("Sun advanced!  from:{0} to:{1}", previousDays, currentDays);
            if (onCycle != null)
                onCycle();
        }
        previousDays = currentDays;
    }

    void OnEnable()
    {
        if (!setTransform)
        {
            origin = transform.forward;
            axis = transform.right;
        }

        Data dat = null;

        if(!reset)
            dat = DataHandler.Read<Data>(System.IO.Path.Combine(Application.persistentDataPath, "sun.dat"));

        if (dat != null)
        {
            time = dat.time;
            light.intensity = dat.intensity;
        }

        previousDays = Mathf.FloorToInt(world.ConvertToDays(time));
    }

    void OnDisable()
    {
        var dat = new Data();
        dat.intensity = light.intensity;
        dat.time = time;

        if(!reset)
            DataHandler.Write<Data>(dat, System.IO.Path.Combine(Application.persistentDataPath, "sun.dat"));
    }

    void UpdateListeners()
    {
        foreach(IReactToSun listener in listeners)
        {
            listener.ReactToTimeOfDay(timeOfDay);
            listener.ReactToDay(days);
        }
    }
}
