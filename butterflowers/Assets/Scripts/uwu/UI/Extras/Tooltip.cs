using System;
using UnityEngine;
using UnityEngine.EventSystems;
using uwu.UI.Behaviors.Visibility;

namespace uwu.UI.Extras
{
	[ExecuteInEditMode]
	public class Tooltip : MonoBehaviour, IPointerExitHandler
	{
		public static System.Action<Tooltip, bool> onTooltipStateUpdate;

		public new UnityEngine.Camera camera;

		public Transform target;
		RectTransform rect;
		ToggleOpacity opacity;

		[SerializeField] Vector3 offset = Vector3.zero;
		[SerializeField] Vector2 padding = Vector2.zero;
		[SerializeField] bool globalOffset = true;

		[SerializeField] bool tracking = false, fitInCanvas = true, closeOnPointerExit = false;
		UnityEngine.Camera mainCamera;

		public bool @override = false;

		bool m_active = false;

		public bool active
		{
			get { return m_active;  }
			set
			{
				m_active = value;
			}
		}

		public RectTransform Rect => rect;
		public Vector3 position;

		void Awake()
		{
			mainCamera = UnityEngine.Camera.main;
			rect = GetComponent<RectTransform>();
			opacity = GetComponent<ToggleOpacity>();
		}

		void Update()
		{
			if (active) {
				if (target == null) 
					return;

				position = CalculatePosition();
			}
		}

		void OnDisable()
		{
			if (active) {
				if (onTooltipStateUpdate != null)
					onTooltipStateUpdate(this, false); // Declare inactive
			}
		}

		void LateUpdate()
		{
			if (active && fitInCanvas) 
			{
				var pos = rect.position;
				var bounds = new Vector2(Screen.width, Screen.height);
				
				var x = pos.x;
				var y = pos.y;

				var w = rect.rect.width / 2f;
				var h = rect.rect.height / 2f;

				x = Mathf.Clamp(x, padding.x + w/2f, (bounds.x) - padding.x - w/2f);
				y = Mathf.Clamp(y, padding.y + h/2f, (bounds.y) - padding.y - h/2f);
				
				rect.position = new Vector2(x, y);
			}
		}

		Vector3 CalculatePosition()
		{
			if (tracking) 
				return Input.mousePosition;

			var cam = camera == null ? mainCamera : camera;

			if (globalOffset)
				return cam.WorldToScreenPoint(target.position) + offset;
			
			return cam.WorldToScreenPoint(target.position + target.TransformVector(offset));
		}

		public void Show()
		{
			if (!active) 
			{
				position = CalculatePosition();
				transform.position = position;
				
				if (onTooltipStateUpdate != null)
					onTooltipStateUpdate(this, true);
			}
			
			active = true;
			opacity.Show();
		}

		public void Hide(bool force = false)
		{
			if (force || !closeOnPointerExit) 
			{
				if (active) {
					if (onTooltipStateUpdate != null)
						onTooltipStateUpdate(this, false);
				}
				
				active = false;
				opacity.Hide();
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if(closeOnPointerExit && active)
				Hide(force:true);
		}
	}
}