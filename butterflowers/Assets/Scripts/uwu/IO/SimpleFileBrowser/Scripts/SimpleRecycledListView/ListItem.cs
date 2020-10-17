using UnityEngine;

namespace uwu.IO.SimpleFileBrowser.Scripts.SimpleRecycledListView
{
	[RequireComponent(typeof(RectTransform))]
	public class ListItem : MonoBehaviour
	{
		IListViewAdapter adapter;
		public object Tag { get; set; }
		public int Position { get; set; }

		internal void SetAdapter(IListViewAdapter listView)
		{
			adapter = listView;
		}

		public void OnClick()
		{
			if (adapter.OnItemClicked != null)
				adapter.OnItemClicked(this);
		}
	}
}