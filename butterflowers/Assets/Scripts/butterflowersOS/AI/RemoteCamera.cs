using System;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Menu;
using UnityEngine;
using Cinemachine;
using uwu.Camera;

namespace butterflowersOS.AI
{
	public class RemoteCamera : GameCamera
	{
		#region Internal

		public enum State
		{
			Wait,
			
			Pan,
			Release
		}
		
		#endregion
		
		// External

		[SerializeField] PauseMenu pauseMenu;
		
		// Properties

		[SerializeField] float radius = 0f;
		[SerializeField] float height = 0f;
		[SerializeField] float xAngle = 0f, yAngle = 0f;

		Vector3 mouseA, mouseB;
		Vector3 velocity = Vector3.zero;
		
		public bool ReadInput { get; set; }

		[SerializeField] State _state = State.Wait; 
		
		// Attributes

		[SerializeField] float dampening = 1f;
		[SerializeField] float strength = 1f;
		[SerializeField] int velocityStackSize = 10;

		[SerializeField] float minYAngle = 0f, maxYAngle = 2f * Mathf.PI;
		
		// Collections
		
		List<Vector3> velocities = new List<Vector3>();
		
		
		protected override void Start()
		{
			base.Start();

			Vector3 offset = transposer.m_FollowOffset;
			
			radius = offset.magnitude; // Calculate radius of transposer from anchor

			xAngle = Mathf.Asin(offset.z / radius);
			yAngle = Mathf.Asin(offset.y / radius);
		}

		void Update()
		{
			if (!IsActive || !ReadInput) return;

			bool releaseDuringFrame = false;

			if (!pauseMenu.IsActive) 
			{
				Vector3 mousePosition = Input.mousePosition;

				if ((_state == State.Wait || _state == State.Release) && Input.GetMouseButtonDown(0)) 
				{
					mouseA = mouseB = mousePosition;
					velocity = Vector3.zero;
					velocities = new List<Vector3>();

					_state = State.Pan;
				}
				else if ((_state == State.Pan) && Input.GetMouseButton(0)) 
				{
					mouseB = mousePosition;
					velocity = (mouseB - mouseA) * strength * Time.deltaTime;
					mouseA = mouseB;

					Drag();

					_state = State.Pan;
				}
				else 
				{
					releaseDuringFrame = (_state == State.Pan && Input.GetMouseButtonUp(0));
				}
			}
			else 
			{
				releaseDuringFrame = (_state != State.Wait && _state != State.Release);
			}

			if (releaseDuringFrame) 
			{
				if (_state != State.Release) 
				{
					Propel();
					_state = State.Release;
				}
			}
			else 
			{
				if(_state == State.Release) Release();	
			}

			ApplyOffset();
		}
		
		
		#region Ops

		void Drag()
		{
			xAngle = Mathf.Repeat(xAngle + velocity.x, 2f*Mathf.PI);
			yAngle = Mathf.Clamp(yAngle + velocity.y, minYAngle, maxYAngle);

			int count = velocities.Count;
			if (count >= velocityStackSize) 
				velocities.RemoveAt(0);
			
			velocities.Add(velocity);
		}

		void Propel()
		{
			if (velocities.Count > 0) 
			{
				foreach (Vector3 vel in velocities) 
				{
					float speed = vel.magnitude;
					if (speed > velocity.magnitude) 
					{
						velocity = vel;
					}
				}	
			}
		}

		void Release()
		{
			float dt = Time.deltaTime;
			
			xAngle = Mathf.Repeat(xAngle + velocity.x, 2f*Mathf.PI);
			yAngle = Mathf.Clamp(yAngle + velocity.y, minYAngle, maxYAngle);
			
			velocity *= (1f - (dampening * dt));
		}
		
		#endregion
		
		#region Camera

		void ApplyOffset()
		{
			Vector3 offset = new Vector3(Mathf.Cos(xAngle), Mathf.Sin(yAngle), Mathf.Sin(xAngle)) * radius;
			transposer.m_FollowOffset = offset;
		}
		
		#endregion
	}
}