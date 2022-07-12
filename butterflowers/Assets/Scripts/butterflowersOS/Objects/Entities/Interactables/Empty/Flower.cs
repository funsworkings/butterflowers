using System.Collections;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Managers;
using butterflowersOS.Presets;
using UnityEngine;

namespace butterflowersOS.Objects.Entities.Interactables.Empty
{
	using BeaconType = Beacon.Type;
	
	public class Flower : Interactable, IFlammable, ITooltip
	{
		#region Internal
		
		public enum Origin
		{
			Vine = 0,
			Beacon = 1
		}
		
		#endregion

		// Properties

		[SerializeField] WorldPreset preset;
		[SerializeField] private GameObject root;
		
		public Animator animator;
		[SerializeField] ParticleSystem firePS = null;

		string file;
		BeaconType type;
		public Origin origin;
		
		protected override void Awake()
		{
			base.Awake();
			
			//animator = GetComponent<Animator>();
			animator.SetBool("visible", false);
		}
		
		#region Growth

		public void Grow(Origin origin)
		{
			this.origin = origin;
			
			int profileIndex = (int) origin;

			animator.SetInteger("growProfile", profileIndex);
			animator.SetBool("visible", true);
		}

		public void Grow(Origin origin, string file, BeaconType type)
		{
			this.origin = origin;
			
			Grow(origin);
			
			this.file = file;
			this.type = type;
		}
		
		#endregion
		
		#region Interactable overrides

		protected override void onGrab(Vector3 point, Vector3 normal)
		{
			if (origin != Origin.Beacon) return;
			if (file == null) return;

			SpawnBeacon();
		}

		public Beacon SpawnBeacon()
		{
			var @params = new Hashtable() 
			{
				{ "position" , transform.position }
			};
			
			return FindObjectOfType<BeaconManager>().CreateBeacon(file, type, Beacon.Locale.Terrain, @params, fromSave:false, transition: BeaconManager.TransitionType.Flower);
		}

		#endregion
		
		#region Flammable

		public bool IsOnFire => firePS.isPlaying;
		
		[ContextMenu("Fire")]
		public void Fire()
		{
			firePS.Play();
		}

		[ContextMenu("Extinguish")]
		public void Extinguish()
		{
			firePS.Stop();
		}

		public void Vanish()
		{
			if (origin != Origin.Beacon)
			{
				Extinguish();
				return;
			}
			else
			{
				DestroyImmediate(root);
			}
		}
		
		#endregion

		public string GetInfo()
		{
			return (origin == Origin.Beacon)? file.AppendActionableInformation(this):null;
		}
	}
}