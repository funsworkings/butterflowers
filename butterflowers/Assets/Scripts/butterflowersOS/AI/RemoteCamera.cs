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
		// External

		[SerializeField] PauseMenu pauseMenu;
		
		// Properties

		[SerializeField] float radius = 0f;
		[SerializeField] float height = 0f;
		[SerializeField] float xAngle = 0f, yAngle = 0f;

		Vector3 mouseA, mouseB;
		Vector3 velocity = Vector3.zero;
		
		public bool ReadInput { get; set; }
		
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
			yAngle = Mathf.Asin(offset.y / radius);
		}

		void Update()
		{
			if (!IsActive || !ReadInput) return;

			Vector3 mousePosition = Input.mousePosition;

			bool @down = Input.GetMouseButtonDown(0);
			bool @continue = Input.GetMouseButton(0);
			bool @release = Input.GetMouseButtonUp(0);

			if (@down) 
			{
				mouseA = mouseB = mousePosition;
				velocity = Vector3.zero;
				velocities = new List<Vector3>();
			}
			else if (@continue) 
			{
				mouseB = mousePosition;
				velocity = (mouseB - mouseA) * strength * Time.deltaTime;
				mouseA = mouseB;
				
				Drag();
			}
			else 
			{
				if (@release) 
				{
					Propel();
				}
				
				Release();	
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