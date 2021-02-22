using System;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Menu;
using UnityEngine;
using Cinemachine;
using uwu.Camera;
using uwu.Extensions;

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

		Rigidbody _rigid;
		
		Vector3 anchor = Vector3.zero;
		
		public bool ReadInput { get; set; }

		[SerializeField] State _state = State.Wait; 
		
		// Attributes

		[SerializeField] float dampening = 1f;
		[SerializeField] float strength = 1f;
		[SerializeField] int velocityStackSize = 10;

		[SerializeField] float minYAngle = 0f, maxYAngle = 2f * Mathf.PI;

		[Header("Free look")]
			[SerializeField] Quaternion _rootAngle;
			[SerializeField] Vector2 _rotation = Vector2.zero;
			[SerializeField] bool invertX = false, invertY = false;
			[SerializeField] float rotationSpeedX = 1f, rotationSpeedY = 1f;
			[SerializeField] float minRotationY = -89f, maxRotationY = 89f;
			[SerializeField] int rotationSmoothFrames = 1;

		[Header("Free move")] 
			[SerializeField] float acceleration = 1f, deceleration = 1f;
			[SerializeField] Vector3 _velocity = Vector3.zero;
			[SerializeField] float maxSpeed = 1f;

		// Collections
		
		List<Vector3> velocities = new List<Vector3>();
		List<Vector2> rotationFrames = new List<Vector2>();
		

		protected override void Awake()
		{
			base.Awake();

			_rigid = GetComponent<Rigidbody>();
		}
		
		protected override void Start()
		{
			base.Start();

			Vector3 offset = transposer.m_FollowOffset;
			
			radius = offset.magnitude; // Calculate radius of transposer from anchor

			xAngle = Mathf.Asin(offset.z / radius);
			yAngle = Mathf.Asin(offset.y / radius);

			_rootAngle = transform.localRotation;

			_rigid.freezeRotation = true;
		}

		void Update()
		{
			if (!IsActive || !ReadInput) return;

			if (!pauseMenu.IsActive) 
			{
				FreeLook();
				FreeMove();
			}

			_rigid.velocity = _velocity;
			
			//_rigid.MovePosition(transform.position +  * Time.deltaTime);
		}

		#region Freeplay

		public void SwitchToFreeplay()
		{
			transposer.enabled = false;
			composer.enabled = false;
		}

		void FreeLook()
		{
			Vector2 rot = new Vector2(Input.GetAxis("Mouse X") * rotationSpeedX * ((invertX)? -1f:1f), Input.GetAxis("Mouse Y") * rotationSpeedY * ((invertY)? -1f:1f)) * Time.timeScale;
			
			Vector2 r_Vel = Vector2.zero;

			rotationFrames.Add(rot);
			if (rotationFrames.Count >= rotationSmoothFrames)
			{
				rotationFrames.RemoveAt(0);	
			}

			int size = rotationFrames.Count;
			for (int i = 0; i < size; i++) 
			{
				r_Vel += (rotationFrames[i]);
			}
			r_Vel /= size;

			_rotation += r_Vel;
			_rotation.y = Mathf.Clamp(_rotation.y, minRotationY, maxRotationY);

			Quaternion xAngle = Quaternion.AngleAxis(_rotation.x, Vector3.up);
			Quaternion yAngle = Quaternion.AngleAxis(_rotation.y, Vector3.right);
			
			transform.localRotation = _rootAngle * xAngle * yAngle;

		}

		void FreeMove()
		{
			if (Input.GetMouseButton(0)) 
			{
				_velocity += (transform.forward * acceleration * Time.deltaTime);
			}
			else 
			{
				_velocity *= (1f - (Time.deltaTime * deceleration));
			}

			float speed = _velocity.magnitude;
			float dampen = Mathf.Min(maxSpeed / speed, 1f);
			
			_velocity *= dampen;
		}
		
		#endregion
	}
}