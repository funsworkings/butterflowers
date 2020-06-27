using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode.Examples.MathNodes;

public class NestCamera : FocusCamera
{
    [SerializeField] Transform anchor;
    [SerializeField] Nest nest;

    [SerializeField] float smoothMoveSpeed = 1f, smoothLookSpeed = 1f;
    [SerializeField] [Range(0f, 1f)] float nestLookAtWeight = 1f;
    [SerializeField] float distanceFromNest = 5f, anchorHeightOffset = 0f;

    void Update()
    {
        Vector3 anchor_pos = anchor.position + Vector3.up*anchorHeightOffset;
        Vector3 nest_pos = nest.transform.position;

        Vector3 dir = (anchor_pos - nest_pos).normalized;
        Vector3 mid = (anchor_pos * (1f - nestLookAtWeight) + nest_pos * nestLookAtWeight);

        /* * * * * * * * */

        Vector3 c_pos = transform.position;
        Vector3 t_pos = nest_pos - (dir * distanceFromNest);

        transform.position = Vector3.Lerp(c_pos, t_pos, Time.deltaTime * smoothMoveSpeed);

        Quaternion c_rot = transform.rotation;
        Quaternion t_rot = Quaternion.LookRotation((mid - t_pos).normalized, Vector3.up);

        transform.rotation = Quaternion.Lerp(c_rot, t_rot, Time.deltaTime * smoothLookSpeed);
    }
}
