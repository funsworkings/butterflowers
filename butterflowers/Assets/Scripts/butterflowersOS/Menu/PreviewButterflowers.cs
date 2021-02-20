using UnityEngine;
using uwu.Gameplay;

namespace butterflowersOS.Menu
{
	public class PreviewButterflowers : Spawner
	{
		protected override void Start()
		{
			CalculateBounds();
			Spawn(amount);
		}
		
		#region Spawn
		
		protected override void CalculateBounds()
		{
			var col = spawnRoot.GetComponent<Collider>();

			m_center = spawnRoot.InverseTransformPoint(col.bounds.center);
			m_extents = spawnRoot.InverseTransformVector(col.bounds.extents);

			col.enabled = false; // Disable collider after fetching center+bounds
		}
		
		#endregion
	}
}