using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace uwu.Gameplay
{
	using Camera = UnityEngine.Camera;
	
	public abstract class Interacter : MonoBehaviour
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
			[SerializeField] protected bool wait;
			[SerializeField] List<Interactable> interactables = new List<Interactable>();

			
		#region Accessors

		protected virtual LayerMask mask => interactionMask;
		
		#endregion
			

		protected virtual void Start()
		{
			camera = Camera.main;
		}

		protected virtual void Update()
		{
			down = Input.GetMouseButtonDown(0);
			cont = Input.GetMouseButton(0);
			up = Input.GetMouseButtonUp(0);
			wait = (!down && !cont && !up);
			
			/* * * * * * * * * * * * * * * * * * * * * */

			var hits3d = new RaycastHit[] { };
			var hits2d = new RaycastResult[] { };
			
			QueryInteractions(out hits3d, out hits2d);

			var _frameInteractions = ParseInteractions(hits3d, hits2d);
			FilterInteractions(ref _frameInteractions);

			HandleInteractions(_frameInteractions);
			DisposeInteractions(_frameInteractions.Keys.ToList());
			
			interactables = _frameInteractions.Keys.ToList();
		}
		
		#region Interact

		void QueryInteractions(out RaycastHit[] hits, out RaycastResult[] hits2d)
		{
			ray = camera.ScreenPointToRay(origin);

			bool queryAll = (filter == Filter._All);

			if (filter == Filter._3d || queryAll) 
			{
				if (multipleInteractions) {
					hits = Physics.RaycastAll(ray, interactionDistance, mask.value);
				}
				else {
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

		Dictionary<Interactable, RaycastHit> ParseInteractions(RaycastHit[] hits3d, RaycastResult[] hits2d)
		{
			Dictionary<Interactable, RaycastHit> _frameInteractions = new Dictionary<Interactable, RaycastHit>();
			
			if (hits3d != null) 
			{
				foreach(RaycastHit hit in hits3d)
				{
					var @object = hit.collider.gameObject;
					var interactable = @object.GetComponent<Interactable>();

					if (interactable != null && !_frameInteractions.ContainsKey(interactable)) 
						_frameInteractions.Add(interactable, hit);
				}
			}

			if (hits2d != null) 
			{
				foreach (RaycastResult hit2d in hits2d) 
				{
					var hit = RaycastHitFromResult(hit2d);

					var @object = hit2d.gameObject;
					var interactable = @object.GetComponent<Interactable>();

					if (interactable != null)
						_frameInteractions.Add(interactable, hit);
				}
			}

			return _frameInteractions;
		}

		void DisposeInteractions(List<Interactable> _frameInteractables)
		{
			foreach (Interactable interactable in interactables) 
			{
				if(!_frameInteractables.Contains(interactable))
					interactable.Unhover();
			}
		}

		protected virtual void HandleInteractions(Dictionary<Interactable, RaycastHit> _frameInteractions)
		{
			foreach (KeyValuePair<Interactable, RaycastHit> hit in _frameInteractions) 
			{
				var interactable = hit.Key;
				var raycast = hit.Value;

				if(down) { interactable.Grab(raycast); onGrabInteractable(interactable); }
				else if(cont) { interactable.Continue(raycast); onContinueInteractable(interactable);}
				else if(up) { interactable.Release(raycast); onReleaseInteractable(interactable); }
				else interactable.Hover(raycast);
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

		protected IEnumerable<Interactable> FilterInteractablesByType<E>(IEnumerable<Interactable> interactions)
		{
			List<Interactable> filtered = new List<Interactable>();
			
			foreach (Interactable e in interactions) 
			{
				if (e.GetComponent<E>() != null) 
					filtered.Add(e);	
			}

			return filtered;
		}

		protected Interactable FindClosestInteractable(IEnumerable<Interactable> interactables)
		{
			float minDistance = Mathf.Infinity;
			Interactable closest = null;

			foreach (Interactable i in interactables) {
				float d = Vector3.Distance(transform.position, i.transform.position);
				if (d <= minDistance) {
					minDistance = d;
					closest = i;
				}
			}

			return closest;
		}
		
		#endregion
		
		#region Abstract components

		protected abstract Vector3 origin { get; }
		
		protected virtual void FilterInteractions(ref Dictionary<Interactable, RaycastHit> _frameInteractions){}

		protected virtual void onGrabInteractable(Interactable interactable){}
		protected virtual void onContinueInteractable(Interactable interactable){}
		protected virtual void onReleaseInteractable(Interactable interactable){}
		
		#endregion
	}
}