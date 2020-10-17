using UnityEngine;

namespace uwu.Animation
{
	[RequireComponent(typeof(Animator))]
	public class IK : MonoBehaviour
	{
		[SerializeField] Transform m_head_joint;

		[SerializeField] float body = 3f, head = 1f;

		[SerializeField] float smoothLookAtWeightSpeed = 1f, smoothLookAtPositionSpeed = 1f;

		[SerializeField] float m_lookAtWeight, t_lookAtWeight;

		[SerializeField] Vector3 m_lookAtPosition, t_lookAtPosition;

		[SerializeField] bool debugGizmos;
		Animator animator;

		public float lookAtWeight
		{
			get => m_lookAtWeight;
			set => t_lookAtWeight = value;
		}

		public Vector3 lookAtPosition
		{
			get => m_lookAtPosition;
			set
			{
				t_lookAtPosition = value;

				if (lookAtWeight <= .167f)
					m_lookAtPosition = t_lookAtPosition;
			}
		}

		public Vector3 targetLookAtPosition => t_lookAtPosition;

		public Transform headJoint => m_head_joint;

		void Awake()
		{
			animator = GetComponent<Animator>();
		}

		void Update()
		{
		}

		void OnAnimatorIK(int layerIndex)
		{
			var weight = m_lookAtWeight =
				Mathf.Lerp(m_lookAtWeight, t_lookAtWeight, Time.deltaTime * smoothLookAtWeightSpeed);
			var pos = m_lookAtPosition =
				Vector3.Lerp(m_lookAtPosition, t_lookAtPosition, Time.deltaTime * smoothLookAtPositionSpeed);

			animator.SetLookAtPosition(m_lookAtPosition);
			animator.SetLookAtWeight(weight, body, head);
		}
	}
}