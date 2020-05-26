using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleScale : MonoBehaviour
{

    [SerializeField] Vector3 direction = Vector3.up;
    [SerializeField] float speed = 1f;
    [SerializeField] float amount = 1f;

    [SerializeField] bool randomSpeed = false, randomAmount = false;

    Vector3 origin;

    // Start is called before the first frame update
    void Start()
    {
        origin = transform.localScale;

        if (randomSpeed) speed *= Random.Range(0f, 1f);
        if (randomAmount) amount *= Random.Range(0f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * (amount / 2f);
        transform.localScale = origin + (direction * offset);
    }
}
