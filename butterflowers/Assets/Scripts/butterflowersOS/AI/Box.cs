using System;
using System.Collections;
using butterflowersOS.AI.Objects;
using butterflowersOS.Objects.Entities.Interactables;
using UnityEngine;
using uwu.Extensions;
using uwu.Gameplay;
using uwu.Snippets;
using Random = UnityEngine.Random;

namespace butterflowersOS.AI
{
	public class Box : Entity
	{
		// Properties

		[SerializeField] 
		Platform _platform;

		[SerializeField] VFXER vfx;

		Rigidbody rigid;
		ApplyCustomGravity gravity;
		Collider collider;
		Trail trail;

		Ray safeRay;
		RaycastHit safeHit;
		
		// Attributes

		[SerializeField] LayerMask safetyMask;
		[SerializeField] float safeDistance = 10f;
		[SerializeField] float shiftPaddingTime = 1f;
		[SerializeField] float speedLimit = 67f;
		[SerializeField, Range(0f, 1f)] float brake = 0f;
		[SerializeField] float nodeDropRate = 1f;
		
		int velYDirection = 0;
		bool hasPlatform = false;

		public Vector3 Velocity => rigid.velocity;

		protected override void Awake()
		{
			base.Awake();
			
			rigid = GetComponent<Rigidbody>();
			collider = GetComponent<Collider>();
			
			gravity = GetComponent<ApplyCustomGravity>(); 
			if (gravity == null) gravity = gameObject.AddComponent<ApplyCustomGravity>();
			
			trail = GetComponent<Trail>();
		}

		void Start()
		{
			StartCoroutine("Drop");
		}

		protected override void Update()
		{
			base.Update();
			
			CheckForPlatform(); // Is platform underneath?

			int previousVelYDirection = velYDirection;
			float velY = rigid.velocity.y;
			velYDirection = (int)Mathf.Sign(velY);

			if (velY < 0f && !hasPlatform) // Descent! 
			{
				ShiftPlatform();
			} 
			
			DampenVelocity();
		}
		
		#region Safety

		void CheckForPlatform()
		{
			safeRay = new Ray(transform.position, Vector3.down);
			safeHit = new RaycastHit();

			hasPlatform = Physics.Raycast(safeRay, out safeHit, safeDistance, safetyMask.value); // Check for platform
		}

		void ShiftPlatform()
		{
			Vector3 _platformPos = _platform.transform.position;
			if (_platformPos.y < transform.position.y) return; // Ignore request to move platform if lower
			
			Vector3 positionAfterT = rigid.PredictPhysicsPosition(shiftPaddingTime);
			_platform.transform.position = positionAfterT;
		}

		void DampenVelocity()
		{
			Vector3 direction = rigid.velocity.normalized;
			
			float speed = rigid.velocity.magnitude;
			if (speed > speedLimit) 
			{
				float diff = speedLimit - speed;
				rigid.AddForce(direction * diff * brake);
			}
		}
		
		#endregion
		
		#region Wrap

		protected override void OnPostWrap()
		{
			base.OnPostWrap();

			Vector3 gravDir = Random.insideUnitSphere.normalized;
			gravity.directionOfGravity = gravDir;
			
			//trail.Clear();
		}

		#endregion

		#region Ops

		public void Propel(float strength)
		{
			Vector3 sphere_pos = Random.insideUnitSphere;
			Vector3 dir = -sphere_pos;

			rigid.AddForce(dir * strength);
			
			trail.Push(transform.position);
			
			/*
			Vector3 ray_origin = -dir * 5f;
			Vector3 ray_dir = dir;

			var ray = new Ray(transform.position + ray_origin, ray_dir);
			var hit = new RaycastHit();

			if (collider.Raycast(ray, out hit, 10f)) {
				var normal = hit.normal;

				Vector3 position = hit.point;
				Vector3 _dir = (-normal - 3f * Physics.gravity).normalized;

				rigid.AddForce(dir * strength);
			}
			*/
		}

		IEnumerator Drop()
		{
			while (true) 
			{
				var node = vfx.RequestEntity("box_node");
				if(node != null) node.transform.position = transform.position;
				
				yield return new WaitForSeconds(nodeDropRate);
			}
		}
		
		#endregion
	}
}