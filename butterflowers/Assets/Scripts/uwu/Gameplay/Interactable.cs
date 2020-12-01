using System;
using UnityEngine;

namespace uwu.Gameplay
{
	[RequireComponent(typeof(Collider))]
	public class Interactable : MonoBehaviour
	{
		#region Events

		public Action<Vector3, Vector3> onHover, onUnhover, onGrab, onContinue, onRelease;

		#endregion

		#region External interaction

		public void Hover(RaycastHit hit)
		{
			if (onHover != null)
				onHover(hit.point, hit.normal);
		}

		public void Unhover()
		{
			if (onUnhover != null)
				onUnhover(Vector3.zero, Vector3.zero);
		}

		public void Grab(RaycastHit hit)
		{
			if (onGrab != null)
				onGrab(hit.point, hit.normal);
		}

		public void Continue(RaycastHit hit)
		{
			if (onContinue != null)
				onContinue(hit.point, hit.normal);
		}

		public void Release(RaycastHit hit)
		{
			Debug.Log("release " + gameObject.name);
			if (onRelease != null)
				onRelease(hit.point, hit.normal);
		}

		#endregion

		void Start()
		{
			
		}

		void Update()
		{
			
		}
	}
}