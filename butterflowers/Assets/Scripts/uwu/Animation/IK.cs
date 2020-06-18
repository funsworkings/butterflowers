using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class IK : MonoBehaviour
{
    Animator animator;

    float body = 3f, head = 1f;

    [SerializeField] float smoothLookAtWeightSpeed = 1f;

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

    [SerializeField] Vector3 m_lookAtPosition;
    public Vector3 lookAtPosition
    {
        set
        {
            m_lookAtPosition = value;
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update() { }

    void OnAnimatorIK(int layerIndex)
    {
        var weight = m_lookAtWeight = Mathf.Lerp(m_lookAtWeight, t_lookAtWeight, Time.deltaTime * smoothLookAtWeightSpeed);

        animator.SetLookAtPosition(m_lookAtPosition);
        animator.SetLookAtWeight(weight, body, head);
    }
}
