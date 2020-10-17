using UnityEngine;
using UnityEngine.EventSystems;
using uwu.IO.SimpleFileBrowser.Scripts.SimpleRecycledListView;

namespace uwu.IO.SimpleFileBrowser.Scripts
{
	public class FileBrowserMovement : MonoBehaviour
	{
		#region Initialization Functions

		public void Initialize(FileBrowser fileBrowser)
		{
			this.fileBrowser = fileBrowser;
			canvasTR = fileBrowser.GetComponent<RectTransform>();
		}

		#endregion

		#region Variables

#pragma warning disable 0649
		FileBrowser fileBrowser;
		RectTransform canvasTR;
		UnityEngine.Camera canvasCam;

		[SerializeField] RectTransform window;

		[SerializeField] RecycledListView listView;
#pragma warning restore 0649

		Vector2 initialTouchPos = Vector2.zero;
		Vector2 initialAnchoredPos, initialSizeDelta;

		#endregion

		#region Pointer Events

		public void OnDragStarted(BaseEventData data)
		{
			var pointer = (PointerEventData) data;

			canvasCam = pointer.pressEventCamera;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(window, pointer.pressPosition, canvasCam,
				out initialTouchPos);
		}

		public void OnDrag(BaseEventData data)
		{
			var pointer = (PointerEventData) data;

			Vector2 touchPos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(window, pointer.position, canvasCam, out touchPos);
			window.anchoredPosition += touchPos - initialTouchPos;
		}

		public void OnEndDrag(BaseEventData data)
		{
			fileBrowser.EnsureWindowIsWithinBounds();
		}

		public void OnResizeStarted(BaseEventData data)
		{
			var pointer = (PointerEventData) data;

			canvasCam = pointer.pressEventCamera;
			initialAnchoredPos = window.anchoredPosition;
			initialSizeDelta = window.sizeDelta;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTR, pointer.pressPosition, canvasCam,
				out initialTouchPos);
		}

		public void OnResize(BaseEventData data)
		{
			var pointer = (PointerEventData) data;

			Vector2 touchPos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTR, pointer.position, canvasCam,
				out touchPos);

			var delta = touchPos - initialTouchPos;
			var newSize = initialSizeDelta + new Vector2(delta.x, -delta.y);

			if (newSize.x < fileBrowser.minWidth) newSize.x = fileBrowser.minWidth;
			if (newSize.y < fileBrowser.minHeight) newSize.y = fileBrowser.minHeight;

			newSize.x = (int) newSize.x;
			newSize.y = (int) newSize.y;

			delta = newSize - initialSizeDelta;

			window.anchoredPosition = initialAnchoredPos + new Vector2(delta.x * 0.5f, delta.y * -0.5f);
			window.sizeDelta = newSize;

			listView.OnViewportDimensionsChanged();
		}

		public void OnEndResize(BaseEventData data)
		{
			fileBrowser.EnsureWindowIsWithinBounds();
		}

		#endregion
	}
}