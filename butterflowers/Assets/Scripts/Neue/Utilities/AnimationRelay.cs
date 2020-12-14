using Neue.Agent;
using Neue.Agent1;
using UnityEngine;

namespace Neue.Utilities
{
	public class AnimationRelay : MonoBehaviour
	{
		[SerializeField] Body body;
		
		public void OnCastSpell()
		{
			body.OnCastSpell();
		}

		public void OnTeleport()
		{
			body.OnTeleport();
		}
	}
}