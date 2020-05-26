using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class IK : MonoBehaviour
{
    Animator animator;

    float body = 2f, head = 1f;

    [SerializeField] float m_lookAtWeight = 0f;
    public float lookAtWeight
    {
        set
        {
            m_lookAtWeight = value;
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
        var weight = m_lookAtWeight;

        animator.SetLookAtWeight(weight, body, head);
        animator.SetLookAtPosition(m_lookAtPosition);
    }
}
