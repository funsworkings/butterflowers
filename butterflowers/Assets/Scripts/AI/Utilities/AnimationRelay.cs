using AI.Agent;
using UnityEngine;

namespace AI.Utilities
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