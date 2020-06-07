using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class Loading : MonoBehaviour
{
    #region Events

    public UnityEvent onComplete;

    public System.Action<float> onProgress;

	#endregion

	[SerializeField]
	float m_progress = 0f;
    float t_progress = 0f;

    public float progress 
    {
        get
        {
            return m_progress;
        }
        set
        {
            float val = Mathf.Clamp01(value);

            if (smooth)
                t_progress = val;
            else
            {
                m_progress = val;
                onUpdateProgress();
            }
        }
    }

    [SerializeField] bool smooth = false;
    [SerializeField] float smoothSpeed = 1f, minSpeed = -1f, maxSpeed = -1f;

    // Start is called before the first frame update
    void Start()
    {
        m_progress = t_progress = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!smooth) return;

        m_progress = Mathf.Clamp01(m_progress);
        t_progress = Mathf.Clamp01(t_progress);

        if (m_progress >= t_progress) {
            m_progress = t_progress; // Immediately move back if progress reverts
        }

        float speed = (t_progress - m_progress) * smoothSpeed;
        if (minSpeed > 0f) speed = Mathf.Max(speed, minSpeed);
        if (maxSpeed > 0f) speed = Mathf.Min(speed, maxSpeed);

        m_progress = Mathf.Clamp(m_progress + speed*Time.deltaTime, 0f, t_progress);
        onUpdateProgress();
    }

    void onUpdateProgress()
    {
        if (m_progress >= 1f) {
            onComplete.Invoke();
        }
        else 
        {
            if (onProgress != null)
                onProgress(progress);
        }
    }
}
