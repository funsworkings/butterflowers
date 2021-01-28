using butterflowersOS.Interfaces;
using UnityEngine;
using uwu.Gameplay;

namespace butterflowersOS.Objects.Miscellaneous
{
	[RequireComponent(typeof(Renderer))]
	public class SceneMesh : MonoBehaviour, IInteractable, ITooltip
	{
		// Properties
		
		[SerializeField] int _priority = 0;
		public int Priority => _priority;

		new Renderer _renderer;

		[SerializeField] Vector3 _hiddenScale, _shownScale;
		
		
		#region Accessors

		public Vector3 hidden => _hiddenScale;
		public Vector3 shown => _shownScale;

		public bool visible
		{
			get => _renderer.enabled;
			set => _renderer.enabled = value;
		}

		#endregion

		void Awake()
		{
			_renderer = GetComponent<Renderer>();
			
			Vector3 scale = transform.localScale;
			
			_hiddenScale = new Vector3(scale.x, 0f, scale.z);
			_shownScale = scale;
		}

		#region Ops
		
		public void Show(){ transform.localScale = shown; }
		public void Hide(){ transform.localScale = hidden; }
		
		#endregion
		
		#region Interactable

		public void Hover(RaycastHit hit)
		{
			
		}

		public void Unhover()
		{
			
		}

		public void Grab(RaycastHit hit)
		{
			
		}

		public void Continue(RaycastHit hit)
		{
			
		}

		public void Release(RaycastHit hit)
		{
			
		}
		
		#endregion

		public string GetInfo()
		{
			return gameObject.name;
		}
	}
}