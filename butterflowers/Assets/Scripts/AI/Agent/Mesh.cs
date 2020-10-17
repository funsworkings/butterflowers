using System.Collections;
using UnityEngine;

namespace AI.Agent
{
	public class Mesh : MonoBehaviour
	{
		// Properties

		Animator animator;
		
		// Attributes

		public bool inprogress = false;
		
		// Events

		public System.Action DidCastSpell, DidTeleport;
		
		
		#region Monobehaviour callbacks

		void Awake()
		{
			animator = GetComponent<Animator>();
		}
		
		#endregion
		
		#region Actions

		public void SpellAtLocation(Vector3 position)
		{
			inprogress = true;
			StartCoroutine("SpellSequence", position);
		}

		IEnumerator SpellSequence(Vector3 waypoint)
		{
			yield return new WaitForSeconds(1f);
			transform.parent.position = waypoint;
			
			yield return new WaitForSeconds(.5f);
			Spell();
		}

		public void Spell()
		{
			inprogress = true;
			animator.SetTrigger("cast");
		}
		
		#endregion

		#region Animation callbacks

		public void OnCastSpell()
		{
			inprogress = false;
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