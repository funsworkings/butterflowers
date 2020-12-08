using UnityEngine;

namespace Objects.Entities.Interactables.Empty
{
	using BeaconType = Beacon.Type;
	
	public class Flower : Interactable
	{
		#region Internal
		
		public enum Origin
		{
			Vine = 0,
			Beacon = 1
		}
		
		#endregion

		// Properties
		
		Animator animator;

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
				beacons.CreateBeacon(file, type, Beacon.Locale.Terrain, Vector3.zero, load:false);
		}

		#endregion
	}
}