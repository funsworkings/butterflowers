using Neue.Reference.Nodes.Behaviours;
using UnityEngine;
using UnityEngine.UI;
using uwu.Extensions;

namespace Neue.Reference.Nodes.Debug
{
	public class NodeFrame : MonoBehaviour
	{
		// Properties
		
		IReadFrame node;

		RectTransform rect;
		Image image;

		const float defaultSize = 32f;
		
		void Awake()
		{
			rect = GetComponent<RectTransform>();
			image = GetComponent<Image>();
				image.raycastTarget = false;
		}
		
		#region Ops

		public void SetFromNode(IReadFrame node, Camera camera)
		{
			this.node = node;

			if (node == null) 
			{
				image.enabled = false;
				return;
			}
			
			Vector2 point = Vector2.zero;
			if (!node.Object.transform.IsVisible(camera, out point)) 
			{
				image.enabled = false;
				return;
			}

			image.enabled = true;

			var size = defaultSize;
			var obj = node.Object;
			var collider = node.Collider;
			if (collider != null) 
				size = collider.bounds.extents.magnitude;

			var distance = Vector3.Distance(camera.transform.position, obj.transform.position);
			size = Extensions.DistanceAndDiameterToPixelSize(distance, size, camera);

			image.color = node.GetFrame();
			
			rect.position = new Vector3(point.x, point.y, 0f);
			rect.sizeDelta = Vector2.one * size;
		}

		public void SetFromNode(IReadFrame node)
		{
			image.color = node.GetFrame();
		}
		
		#endregion
	}
}