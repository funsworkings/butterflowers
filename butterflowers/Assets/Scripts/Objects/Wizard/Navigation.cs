using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

namespace Wizard {

	public class Navigation: MonoBehaviour {

		#region Events

		public System.Action<Vector3> onMoveToPoint, onLookAtPoint;

		#endregion

		#region Properties

		NavMeshAgent Agent;
		IK ik;

		#endregion

		#region Attributes

		[SerializeField] Vector3 move_position = Vector3.zero;

		[SerializeField] Transform m_lookAt = null, defaultLookAt;
		[SerializeField] Vector3 safeLookAt = Vector3.zero;

		bool moving = false;
		bool looking = false;

		[SerializeField] float turnThreshold = 90f;
		[SerializeField] float minLookAtSpeed = 1f, maxLookAtSpeed = 3f;
		[SerializeField] bool turning = false;

		public bool debug = false;

		[Header("Debug attributes")]
		[SerializeField] float angleBetween = 0f;

		#endregion

		#region Accessors

		public Transform lookAt => m_lookAt;

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			Agent = GetComponent<NavMeshAgent>();
			ik = GetComponentInChildren<IK>();
		}

		void Start()
		{
			ResetLookAtToDefault();
		}

		void Update()
		{
			if (moving) 
				CheckHasArrivedAtDestination();

			if (lookAt != null) 
				FaceTarget();
			else 
				ik.lookAtWeight = 0f;
		}

		#endregion

		#region Accessors

		public float speed => Agent.velocity.magnitude;

		#endregion

		#region Movement

		void CheckHasArrivedAtDestination()
		{
			float dist = Vector3.Distance(transform.position, move_position);

			if (dist <= Agent.stoppingDistance) {

				if (moving) 
				{
					moving = false;
					if (onMoveToPoint != null)
						onMoveToPoint(move_position);
				}
			}
		}

		#endregion

		#region Looking

		void FaceTarget()
		{
			Vector3 t_pos = lookAt.position.MatchY(transform.position);
			Vector3 dir = (t_pos - transform.position).normalized;

			Quaternion lookRot = Quaternion.LookRotation(dir, transform.up);
			float angle = Quaternion.Angle(transform.rotation, lookRot);
			angleBetween = angle;

			turning = (angle >= turnThreshold);

			bool facing = !turning;

			if (turning) {

				float speed = angle.Remap(0f, 180f, minLookAtSpeed, maxLookAtSpeed);
				transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * speed);
			}
			else 
				safeLookAt = lookAt.position;

			ik.lookAtWeight = facing ? 1f : 0f;
			ik.lookAtPosition = safeLookAt;

			// Fire look at event
			if (facing && looking) 
			{
				looking = false;
				if (onLookAtPoint != null)
					onLookAtPoint(lookAt.position);
			}
		}

		#endregion

		#region Operations

		public void MoveTo(Vector3 position)
		{
			move_position = position;
			moving = true;

			Agent.SetDestination(position);
		}

		public void Stop()
		{
			moving = false;
		}

		public void ResetLookAtToDefault()
		{
			LookAt(defaultLookAt);
		}

		public void LookAt(Transform target)
		{
			m_lookAt = target;

			if(target != null)
				looking = true;
		}

		#endregion

		#region Debug

		void OnDrawGizmos()
		{
			if (debug) {

				var headJoint = ik.headJoint;
				if (headJoint != null) {

					float angleBetween = Vector3.Angle(ik.lookAtPosition, ik.targetLookAtPosition);

					if (angleBetween < 3f)
						Gizmos.color = Color.green;
					else if (angleBetween < 30f)
						Gizmos.color = Color.yellow;
					else
						Gizmos.color = Color.red;

					Gizmos.DrawLine(headJoint.position, ik.lookAtPosition);

					Gizmos.color = Color.green;
					Gizmos.DrawLine(headJoint.position, ik.targetLookAtPosition);
					Gizmos.DrawWireSphere(ik.targetLookAtPosition, 3f);

				}
			}
		}

		#endregion

	}

}
