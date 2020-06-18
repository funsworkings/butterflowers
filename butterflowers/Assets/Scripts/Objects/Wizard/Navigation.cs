using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

namespace Wizard {

    public class Navigation: MonoBehaviour {

		#region Events

		public System.Action<Vector3> onReachDestination;

		#endregion

		#region Properties

		NavMeshAgent Agent;

		#endregion

		#region Attributes

		Vector3 move_position = Vector3.zero;
		bool moving = false;

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			Agent = GetComponent<NavMeshAgent>();
		}

		void Update()
		{
			if (moving) CheckHasArrivedAtDestination();
		}

		#endregion

		#region Accessors

		public float speed => Agent.velocity.magnitude;

		#endregion

		#region Internal

		void CheckHasArrivedAtDestination()
		{
			
		}

		float GetSpeedModifier()
		{
			return 0f;
		}

		#endregion

		#region Operations

		public void MoveTo(Vector3 position)
		{
			Agent.SetDestination(position);
		}

		#endregion

	}

}
