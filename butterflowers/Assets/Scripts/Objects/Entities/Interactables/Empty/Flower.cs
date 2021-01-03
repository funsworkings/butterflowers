using System.Collections;
using Interfaces;
using Objects.Base;
using Settings;
using UnityEngine;

namespace Objects.Entities.Interactables.Empty
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
		
		Animator animator;
		[SerializeField] ParticleSystem firePS;

		string file;
		BeaconType type;
		
		protected override void Awake()
		{
			base.Awake();
			
			animator = GetComponent<Animator>();
			animator.SetBool("visible", false);
		}
		
		#region Growth

		public void Grow(Origin origin)
		{
			int profileIndex = (int) origin;
			
			animator.SetInteger("growProfile", profileIndex);
			animator.SetBool("visible", true);
		}

		public void Grow(Origin origin, string file, BeaconType type)
		{
			Grow(origin);
			
			this.file = file;
			this.type = type;
		}
		
		#endregion
		
		#region Interactable overrides

		protected override void onGrab(Vector3 point, Vector3 normal)
		{
			if (file == null) return;
			
			var beacons = FindObjectOfType<BeaconManager>();
			var @params = new Hashtable() 
			{
				{ "position" , transform.position }
			};
			
			beacons.CreateBeacon(file, type, Beacon.Locale.Terrain, @params, fromSave:false, transition: BeaconManager.TransitionType.Flower);
		}

		#endregion
		
		#region Flammable

		public bool IsOnFire => firePS.isPlaying;
		
		public void Fire()
		{
			firePS.Play();
		}

		public void Extinguish()
		{
			firePS.Stop();
		}
		
		#endregion

		public override string GetInfo()
		{
			return file.AppendActionableInformation(this);
		}
	}
}