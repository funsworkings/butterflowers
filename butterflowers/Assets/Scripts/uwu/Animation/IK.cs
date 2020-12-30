using System;
using UnityEditor;
using UnityEngine;

namespace uwu.Animation
{
	[RequireComponent(typeof(Animator))]
	public class IK : MonoBehaviour
	{
		// Properties
		
		Animator animator;
		
		[SerializeField] Transform _head;
		[SerializeField] Transform lookAtTarget = null;

		[SerializeField] float weight;
		[SerializeField] Vector3 position;

		// Attributes
		
		[SerializeField] float bodyPriority = 3f;
		[SerializeField] float headPriority = 1f;

		[SerializeField] float smoothLookAtWeightSpeed = 1f, smoothLookAtPositionSpeed = 1f;

		
		[SerializeField] float t_lookAtWeight;
		[SerializeField] Vector3 t_lookAtPosition;
		
		
		#region Accessors

		public Transform head => _head;
		
		public float lookAtWeight
		{
			get => weight;
			set => t_lookAtWeight = value;
		}

		public Vector3 lookAtPosition
		{
			get => position;
			private set => t_lookAtPosition = value;
		}

		public Vector3 targetLookAtPosition => t_lookAtPosition;
		public Vector3 resetPosition => head.position + heading * 1.33f;
		public Vector3 heading => transform.forward;
		
		public Transform LookAtTarget { get => lookAtTarget; set { lookAtTarget = value; } }

		#endregion

		void Awake()
		{
			animator = GetComponent<Animator>();
		}

		void Update()
		{
			if (lookAtTarget == null) weight = 0f;

			if (weight <= 0f) lookAtPosition = resetPosition;
			else lookAtPosition = lookAtTarget.position;
			
			SmoothLookAtParams();
		}
		
		void OnAnimatorIK(int layerIndex)
		{
			animator.SetLookAtPosition(lookAtPosition);
			animator.SetLookAtWeight(weight, bodyPriority, headPriority);
		}

		#region Smoothing

		void SmoothLookAtParams()
		{
			weight = Mathf.Lerp(weight, t_lookAtWeight, Time.deltaTime * smoothLookAtWeightSpeed);
			position = Vector3.Lerp(position, t_lookAtPosition, Time.deltaTime * smoothLookAtPositionSpeed);
		}
		
		#endregion
	}
	
	#if UNITY_EDITOR

	[CustomEditor(typeof(IK))]
	public class IKEditor : Editor
	{
		void OnSceneGUI()
		{
			IK ik = (IK) target;
			
			Handles.color = Color.yellow;

			if (ik.lookAtWeight <= 0f) 
			{
				Handles.DrawLine(ik.head.position, ik.resetPosition);
			}
			else {
				var start = ik.head.position;
				var end = ik.targetLookAtPosition;
				var direction = (end - start);
				
				Handles.DrawWireArc(start,  ik.head.up, end, Vector3.SignedAngle(ik.heading, direction.normalized, ik.head.up), direction.magnitude);
			}
		}
	}
	
	#endif
}