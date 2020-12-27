using System;
using Neue.Agent;
using Neue.Agent1;
using UnityEngine;

namespace Neue.Utilities
{
	[Obsolete("Obsolete agent!", true)]
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