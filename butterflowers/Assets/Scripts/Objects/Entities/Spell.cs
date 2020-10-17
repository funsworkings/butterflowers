using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
    public static System.Action<Transform, bool> Arrived;

    ParticleSystem particles;
    Vector3 origin, target;
    Transform waypoint = null;
    bool track = false;

    [SerializeField] float duration = 1f;
    [SerializeField] float height = 1f;

    [SerializeField] AnimationCurve heightCurve;
    [SerializeField] AnimationCurve moveCurve;
    

    void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    public void MoveTo(Transform target, bool tracking)
    {
        this.waypoint = target;
        this.target = target.position;
        this.origin = transform.position;
        this.track = tracking;

        particles.Play();
        StartCoroutine("Moving");
    }

    IEnumerator Moving()
    {
        float t = 0f;
        float interval = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            var percent = Mathf.Clamp01(t / duration);
            interval = moveCurve.Evaluate(percent);

            if (track) target = waypoint.position; // Continuous track to target

            transform.position = Vector3.Lerp(origin, target, interval) + Vector3.up * height * heightCurve.Evaluate(percent);
            yield return null;
        }

        var main = particles.main;
        main.loop = false;

        bool success = ContainsTarget();
        if (Arrived != null)
            Arrived(waypoint, success);
    }

    bool ContainsTarget()
    {
        var overlaps = Physics.OverlapSphere(target, 3f);
        for (int i = 0; i < overlaps.Length; i++) {
            if (overlaps[i].transform == waypoint)
                return true;
        }

        return false;
    }
}
