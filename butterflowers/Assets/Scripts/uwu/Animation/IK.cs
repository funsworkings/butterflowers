using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class IK : MonoBehaviour
{
    Animator animator;
    [SerializeField] Transform m_head_joint;

    [SerializeField] float body = 3f, head = 1f;

    [SerializeField] float smoothLookAtWeightSpeed = 1f, smoothLookAtPositionSpeed = 1f;

    [SerializeField] float m_lookAtWeight = 0f, t_lookAtWeight = 0f;
    public float lookAtWeight
    {
        get
        {
            return m_lookAtWeight;
        }
        set
        {
            t_lookAtWeight = value;
        }
    }

    [SerializeField] Vector3 m_lookAtPosition, t_lookAtPosition;
    public Vector3 lookAtPosition
    {
        get
        {
            return m_lookAtPosition;
        }
        set
        {
            t_lookAtPosition = value;

            if (lookAtWeight <= .167f)
                m_lookAtPosition = t_lookAtPosition;
        }
    }

    public Vector3 targetLookAtPosition => t_lookAtPosition;

    public Transform headJoint => m_head_joint;

    [SerializeField] bool debugGizmos = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update() { }

    void OnAnimatorIK(int layerIndex)
    {
        var weight = m_lookAtWeight = Mathf.Lerp(m_lookAtWeight, t_lookAtWeight, Time.deltaTime * smoothLookAtWeightSpeed);
        var pos = m_lookAtPosition = Vector3.Lerp(m_lookAtPosition, t_lookAtPosition, Time.deltaTime * smoothLookAtPositionSpeed);

        animator.SetLookAtPosition(m_lookAtPosition);
        animator.SetLookAtWeight(weight, body, head);
    }
}
