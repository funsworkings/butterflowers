using butterflowersOS.Objects.Base;
using UnityEngine;
using uwu.Gameplay;

namespace butterflowersOS.Core.Agent.Modules
{
	public class FOV : MonoBehaviour
	{
		// External
		
		[SerializeField] Wand Wand;
		
		
		#region Queries

		public Entity GetCurrentEntity()
		{
			var _interactable = Wand.CurrentInteractableCollider;

			if (_interactable != null)
				return _interactable.GetComponent<Entity>();

			return null;
		}
		
		#endregion
	}
}