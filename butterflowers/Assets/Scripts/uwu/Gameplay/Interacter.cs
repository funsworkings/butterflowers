using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace uwu.Gameplay
{
	using Camera = UnityEngine.Camera;
	
	public abstract class Interacter<E> : MonoBehaviour where E:MonoBehaviour
	{
		#region Internal

		public enum Filter
		{
			_2d,
			_3d,
			_All
		}
		
		#endregion
		
		// Properties

		protected new Camera camera;
		
		protected Ray ray;
		protected RaycastHit raycastHit;
		
		// Collections
		
		protected Dictionary<IInteractable, RaycastHit> frameInteractions = new Dictionary<IInteractable, RaycastHit>();

		// Attributes

		[Header("General")] 
			[SerializeField] Filter filter = Filter._3d;
			[SerializeField] protected LayerMask interactionMask;
			[SerializeField] protected float interactionDistance = 100f;
			[SerializeField] bool multipleInteractions = true;

		[Header("Debug")]
			[SerializeField] protected bool down;
			[SerializeField] protected bool cont;
			[SerializeField] protected bool up;


		protected virtual void Start()
		{
			camera = Camera.main;
		}

		protected virtual void Update()
		{
			down = Input.GetMouseButtonDown(0);
			cont = Input.GetMouseButton(0);
			up = Input.GetMouseButtonUp(0);

			/* * * * * * * * * * * * * * * * * * * * * */

			var hits3d = new RaycastHit[] { };
			var hits2d = new RaycastResult[] { };
			
			QueryInteractions(out hits3d, out hits2d);
			
			/* * * * * * * * * * * * * * * * * * * * * */
			
			Dictionary<IInteractable, RaycastHit> _frameInteractions = new Dictionary<IInteractable, RaycastHit>();
			List<E> _frameEntities = new List<E>();

			ParseInteractions(ref _frameInteractions, hits3d, hits2d);
			FilterInteractions(ref _frameInteractions);

			_frameInteractions.OrderBy((_frameInteraction) =>
				_frameInteraction.Key.Priority);

			HandleInteractions(_frameInteractions);
			DisposeInteractions(_frameInteractions);
			
			frameInteractions = _frameInteractions;
		}

		#region Interact

		protected virtual void QueryInteractions(out RaycastHit[] hits, out RaycastResult[] hits2d)
		{
			ray = camera.ScreenPointToRay(origin);

			bool queryAll = (filter == Filter._All);

			if (filter == Filter._3d || queryAll) 
			{
				if (multipleInteractions) 
				{
					hits = Physics.RaycastAll(ray, interactionDistance, mask.value);
				}
				else 
				{
					var hit = new RaycastHit();
					if (Physics.Raycast(ray, out hit, interactionDistance, mask.value)) 
						hits = new RaycastHit[] {hit};
					else
						hits = new RaycastHit[]{};
				}
			}
			else
				hits = new RaycastHit[]{};

			if (filter == Filter._2d || queryAll) 
			{
				var ray_2d = new PointerEventData(EventSystem.current);
				ray_2d.position = origin;

				var temp_hits_2d = new List<RaycastResult>();
				EventSystem.current.RaycastAll(ray_2d, temp_hits_2d);

				hits2d = temp_hits_2d.ToArray();
			}
			else 
				hits2d = new RaycastResult[]{};
		}

		void ParseInteractions(ref Dictionary<IInteractable, RaycastHit> _frameInteractions, RaycastHit[] hits3d, RaycastResult[] hits2d)
		{
			if (hits3d != null) 
			{
				foreach(RaycastHit hit in hits3d)
				{
					var @object = hit.collider.gameObject;
					var interactable = @object.GetComponent<IInteractable>();

					if (interactable.IsValid() && !_frameInteractions.ContainsKey(interactable)) 
					{
						_frameInteractions.Add(interactable, hit);
					}
				}
			}

			if (hits2d != null) 
			{
				foreach (RaycastResult hit2d in hits2d) 
				{
					var hit = RaycastHitFromResult(hit2d);

					var @object = hit2d.gameObject;
					var interactable = @object.GetComponent<IInteractable>();

					if (interactable.IsValid() && !_frameInteractions.ContainsKey(interactable)) 
					{
						_frameInteractions.Add(interactable, hit);
					}
				}
			}
		}
		
		protected virtual void HandleInteractions(Dictionary<IInteractable, RaycastHit> _frameInteractions)
		{
			foreach (KeyValuePair<IInteractable, RaycastHit> hit in _frameInteractions) 
			{
				var interactable = hit.Key;
				var raycast = hit.Value;

				if (interactable.IsValid()) 
				{
					if (down) {
						interactable.Grab(raycast);
						onGrabInteractable(interactable, raycast);
					}
					else if (cont) {
						interactable.Continue(raycast);
						onContinueInteractable(interactable, raycast);
					}
					else if (up) {
						interactable.Release(raycast);
						onReleaseInteractable(interactable, raycast);
					}
					else interactable.Hover(raycast);
				}
			}
		}
		
		void DisposeInteractions(Dictionary<IInteractable, RaycastHit> _frameInteractables)
		{
			foreach (KeyValuePair<IInteractable, RaycastHit> interaction in frameInteractions) 
			{
				if (!_frameInteractables.ContainsKey(interaction.Key)) {
					var interactable = interaction.Key;
					if(interactable.IsValid())
						interactable.Unhover();
				}
			}
		}

		#endregion

		#region Helpers

		RaycastHit RaycastHitFromResult(RaycastResult hit2d)
		{
			RaycastHit hit = new RaycastHit();
			GameObject @object = hit2d.gameObject;

			if (@object != null) 
			{
				hit.point = @object.transform.position;
				hit.normal = @object.transform.forward;
			}

			return hit;
		}

		#endregion
		
		#region Abstract components

		protected abstract Vector3 origin { get; }
		protected virtual LayerMask mask => interactionMask;
		
		protected virtual void FilterInteractions(ref Dictionary<IInteractable, RaycastHit> _frameInteractions){}

		protected virtual void onGrabInteractable(IInteractable interactable, RaycastHit hit){}
		protected virtual void onContinueInteractable(IInteractable interactable, RaycastHit hit){}
		protected virtual void onReleaseInteractable(IInteractable interactable, RaycastHit hit){}
		
		#endregion
	}
}