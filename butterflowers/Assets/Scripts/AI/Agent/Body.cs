﻿using System.Collections;
using UnityEngine;
using uwu;

namespace AI.Agent
{
	public class Body : Focusable
	{
		// Events

		/// <summary>
		/// Event when fast traveling [ distance ]
		/// </summary>
		public System.Action<float> onTeleport;
		
		// Properties

		Animator animator;
		ParticleSystem ps;
		Transform root;
		new SkinnedMeshRenderer renderer;
		
		[SerializeField] Brain Brain;
		
		// Attributes

		public bool cast = false;

		public bool teleport = false;
		public bool spell = false;
		
		public bool attached = false;
		Vector3 attachOffset = Vector3.zero;
		
		// Events

		public System.Action DidCastSpell, DidTeleport;
		
		
		#region Monobehaviour callbacks

		protected override void Awake()
		{
			base.Awake();
			
			animator = GetComponentInChildren<Animator>();
			ps = GetComponentInChildren<ParticleSystem>();
			renderer = GetComponentInChildren<SkinnedMeshRenderer>();
		}

		protected override void OnStart()
		{
			defaultTooltipText = GameDataSaveSystem.Instance.username;
			root = transform.parent;
			
			base.OnStart();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (attached) 
			{
				TrackWithAnchor();
			}
		}

		#endregion
		
		#region Verify active

		protected override bool EvaluateActiveState()
		{
			return Brain.Self;
		}

		#endregion
		
		#region Actions

		public void TeleportToLocation(Transform root)
		{
			teleport = true;
			StartCoroutine("TeleportSequence", root);
		}

		IEnumerator TeleportSequence(Transform waypoint)
		{
			yield return new WaitForSeconds(1f);

			var root = waypoint;
			if (root != null) 
			{
				ps.Play();
				renderer.enabled = false;
				yield return new WaitForEndOfFrame();

				yield return new WaitForSeconds(1.3f);

				if (root != null) 
				{
					var distance = Vector3.Distance(transform.position, root.position);
					transform.position = waypoint.position;

					if (onTeleport != null)
						onTeleport(distance);

					ps.Play();
					renderer.enabled = true;
					
					yield return new WaitForSeconds(1.3f);
					teleport = false;
				}
				else 
				{
					teleport = false;
				}
			}
			else 
			{
				teleport = false;
			}
		}

		public void Spell()
		{
			spell = true;
			animator.SetTrigger("cast");
		}

		public bool Attach(Focusable focus)
		{
			if (focus == this) 
			{
				return Detach(); // Detach self is self-focus
			}
			
			Transform root = focus.transform;
			bool success = transform.parent != root;

			transform.parent = root;
			attached = true;

			if (success) 
			{
				Vector3 offset = root.InverseTransformPoint(focus.Anchor.position);
				attachOffset = offset;
				
				TrackWithAnchor(); // Bind immediately
			}
			
			return success;
		}

		void TrackWithAnchor()
		{
			transform.up = Vector3.up;
			transform.position = transform.parent.position + attachOffset;
		}

		public bool Detach()
		{
			bool success = transform.parent != root;

			transform.parent = root;
			attached = false;
			
			return success;
		}
		
		#endregion

		#region Animation callbacks

		public void OnCastSpell()
		{
			cast = true;

			if (DidCastSpell != null)
				DidCastSpell();
		}

		public void OnTeleport()
		{
			if (DidTeleport != null)
				DidTeleport();
		}
		
		#endregion

	}
}