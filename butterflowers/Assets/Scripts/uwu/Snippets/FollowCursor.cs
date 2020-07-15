using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FollowCursor : MonoBehaviour
{
    [Range(0f, 1f)] public float lerpAmount = 1f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        Track();
    }

    void Track()
    {
        Vector3 current = transform.position;
        Vector3 target = Input.mousePosition;

        Vector3 dir = (target - current);

        transform.position = current + dir * lerpAmount;
    }
}
