using UnityEngine;

namespace uwu.Gameplay
{
	public interface IInteractable
	{
		void Hover(RaycastHit hit);
		void Unhover();
		
		void Grab(RaycastHit hit);
		void Continue(RaycastHit hit);
		void Release(RaycastHit hit);
	}

	public static class IInteractableExtensions
	{
		public static bool IsValid(this IInteractable interactable)
		{
			if (interactable == null) return false;

			if (interactable is Object) 
			{
				return interactable as Object;
			}

			return true;
		}
	}
}