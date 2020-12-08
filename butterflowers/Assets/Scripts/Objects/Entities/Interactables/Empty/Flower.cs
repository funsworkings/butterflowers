using UnityEngine;

namespace Objects.Entities.Interactables.Empty
{
	public class Flower : Interactable
	{
		#region Internal
		
		public enum GrowProfile
		{
			Normal = 0,
			Small = 1
		}
		
		#endregion

		// Properties
		
		Animator animator;

		string file;
		Beacon.Type type;
		
		protected override void Awake()
		{
			base.Awake();
			
			animator = GetComponent<Animator>();
			animator.SetBool("visible", false);
		}
		
		#region Growth

		public void Grow(GrowProfile profile)
		{
			int profileIndex = (int) profile;
				animator.SetInteger("growProfile", profileIndex);
				animator.SetBool("visible", true);
		}

		public void Grow(GrowProfile profile, string file, Beacon.Type type)
		{
			Grow(profile);
			
			this.file = file;
			this.type = type;
		}
		
		#endregion
		
		#region Interactable overrides

		protected override void onGrab(Vector3 point, Vector3 normal)
		{
			if (file == null) return;
			
			var beacons = FindObjectOfType<BeaconManager>();
				beacons.CreateBeacon(file, type, Beacon.Locale.Terrain, Vector3.zero);
		}

		#endregion
	}
}