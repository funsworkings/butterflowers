using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleOscillate : MonoBehaviour
{
    [SerializeField] Vector3 axis = Vector3.up;
    [SerializeField] float delay = 0f;

    public float speed = 1f, magnitude = 1f;

    [SerializeField] bool randomSpeed = false, randomMagnitude = false, randomDelay = false;

    bool oscillating = false;
    Vector3 root = Vector3.zero;
    float t = 0f;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if(randomDelay) delay = Random.Range(0f, delay);
        yield return new WaitForSeconds(delay);

        if(randomSpeed)
            speed = Random.Range(0f, speed);
        if(randomMagnitude)
            magnitude = Random.Range(0f, magnitude);

        //axis = transform.InverseTransformDirection(axis.normalized

        root = transform.localPosition;
        axis = axis.normalized;

        oscillating = true;
    }

    void OnEnable()
    {
        if(oscillating) root = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(!oscillating) return;

        transform.localPosition = root + magnitude * Mathf.Sin(t) * axis;
        t += (Time.deltaTime * speed);
    }
}
