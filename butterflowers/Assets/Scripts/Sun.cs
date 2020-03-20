using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
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


    void Awake()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime; // Add time to global clock

        AlignWithRay();
    }

    void AlignWithRay()
    {
        var bearing = transform.forward;
        var directionBetween = (ray - transform.forward);

        transform.forward += (directionBetween * Mathf.Min(1f, Time.realtimeSinceStartup / (world.ConvertToDays(time))));
    }

    void AdjustRayFromTime() {
        if (world == null)
            return;

        float interval = Mathf.Repeat(time / world.secondsPerDay, 1f);
        float angle = interval * 360f;

        ray = Quaternion.AngleAxis(angle, axis) * origin;
    }


    void OnEnable()
    {
        if (!setTransform)
        {
            origin = transform.forward;
            axis = transform.right;
        }


        var dat = DataHandler.Read<Data>(System.IO.Path.Combine(Application.persistentDataPath, "sun.dat"));
        if (dat != null)
        {
            time = dat.time;
            light.intensity = dat.intensity;
        }
    }

    void OnDisable()
    {
        var dat = new Data();
        dat.intensity = light.intensity;
        dat.time = time;

        DataHandler.Write<Data>(dat, System.IO.Path.Combine(Application.persistentDataPath, "sun.dat"));
    }
}
