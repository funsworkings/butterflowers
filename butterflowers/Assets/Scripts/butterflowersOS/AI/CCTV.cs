using System;
using System.Collections.Generic;
using butterflowersOS.AI.Objects;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using uwu.Snippets;

namespace butterflowersOS.AI
{
	public class CCTV : MonoBehaviour
	{
		CinemachineVirtualCamera camera;
		ApplyCustomGravity gravity;

		[SerializeField] Driver driver;
		[SerializeField] Box box;


		public enum TransitionType
		{
			Predictive,
			Tracking,
			Revolve
		}

		[Header("FOV")]
		[SerializeField] float lookaheadDistance = 3f, stepbackDistance = 10f;
		[SerializeField, Range(0f, 1f)] float focusWeight = 1f;
		[SerializeField, Range(0f, 1f)] float eventWeight = 0f;

		[Header("Debug")] [SerializeField] float debugDrawRadius = 1f;

		Vector3 eventPt, focusPt, targetPt;

		void Awake()
		{
			camera = GetComponent<CinemachineVirtualCamera>();
		}

		void OnEnable()
		{
			box.onWrap += Transition;
		}

		void OnDisable()
		{
			box.onWrap -= Transition;
		}
		
		#if UNITY_EDITOR

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(eventPt, debugDrawRadius);
			
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(focusPt, debugDrawRadius);
			
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(targetPt, debugDrawRadius);

			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(transform.position, targetPt);
		}
		
		#endif

		#region Transition

		void Transition(Vector3 wrapB)
		{
			Vector3 vel = box.Velocity.normalized;
			Vector3 focusPt = wrapB + (vel * lookaheadDistance);
			Vector3 eventPt = Vector3.zero;
			
			List<Point> eventStack = driver.EventStack;
			foreach (Point pt in eventStack) 
			{
				eventPt += pt.position;
			}
			eventPt /= (eventStack.Count);
			
			float tw = (focusWeight + eventWeight);

			Vector3 origin = UnityEngine.Random.insideUnitSphere * stepbackDistance;
			
			this.eventPt = eventPt;
			this.focusPt = focusPt;

			Vector3 target = targetPt = ((eventWeight * this.eventPt) + (focusWeight * this.focusPt)) / tw;
		

			transform.position = origin;
			transform.rotation = Quaternion.LookRotation((target - origin).normalized, Vector3.up);
		}
		
		#endregion
	}
}