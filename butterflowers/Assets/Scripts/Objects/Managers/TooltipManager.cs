using System;
using UnityEngine;
using uwu.UI.Extras;
using System.Collections.Generic;
using System.Linq;

namespace Objects.Managers
{
	public class TooltipManager : MonoBehaviour
	{
		// Properties

		[SerializeField] bool @overrides = false;
		[SerializeField] float safeArclength = 0f;
		
		// Attributes

		[SerializeField] float tooltipSmoothSpeed = 3f;
		[SerializeField] float tooltipPadding = 0f;
		[SerializeField] float baseAngleOffset = 0f;
		
		// Collections
		
		public List<Tooltip> activeTooltips = new List<Tooltip>();
		
		#region Monobehaviour callbacks

		void OnEnable()
		{
			Tooltip.onTooltipStateUpdate += onTooltipUpdate;
		}

		void OnDisable()
		{
			Tooltip.onTooltipStateUpdate -= onTooltipUpdate;
		}

		void Update()
		{
			if (@overrides) 
			{
				RadialTooltips();
			}
			else {
				foreach (Tooltip t in activeTooltips) 
					UpdateTooltipPosition(t, Vector3.zero);
			}
		}

		#endregion
		
		#region Tooltip priorities

		void onTooltipUpdate(Tooltip tooltip, bool active)
		{
			if (active)
				activeTooltips.Add(tooltip);
			else
				activeTooltips.Remove(tooltip);

			@overrides = CheckForMultipleTooltips();
			foreach (Tooltip t in activeTooltips)
				t.@override = @overrides;

			if (@overrides) 
			{
				safeArclength = CalculateMaximumDimension() * (tooltipPadding+1f);
			}
		}
		
		#endregion
		
		#region Tooltip positioning

		bool CheckForMultipleTooltips()
		{
			return activeTooltips.Count > 1;
		}

		void RadialTooltips()
		{
			float angle = baseAngleOffset * Mathf.Deg2Rad;
			int segments = activeTooltips.Count;
			float angleOffset = 2f * Mathf.PI / segments;
			float radius = safeArclength / (angleOffset);
			
			// 2 pi r * (ANGLE / 2PI)

			for (int i = 0; i < segments; i++) {
				var tooltip = activeTooltips[i];

				var x = Mathf.Cos(angle) * radius;
				var y = Mathf.Sin(angle) * radius;
				
				UpdateTooltipPosition(tooltip, new Vector2(x, y));
				
				angle += angleOffset;
				angle = Mathf.Repeat(angle, 2f * Mathf.PI);
			}
		}

		float CalculateMaximumDimension()
		{
			float max = 0f;
			foreach (Tooltip t in activeTooltips) {
				var rect = t.Rect.rect;
				max = Mathf.Max(max, rect.width, rect.height);
			}

			return max;
		}

		void UpdateTooltipPosition(Tooltip tooltip, Vector3 offset)
		{
			var target = (@overrides)? Input.mousePosition + offset : tooltip.position;
			tooltip.Rect.position = Vector3.Lerp(tooltip.Rect.position, target, Time.deltaTime * tooltipSmoothSpeed);
		}
		
		#endregion
	}
}