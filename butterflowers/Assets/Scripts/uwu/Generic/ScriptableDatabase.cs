using System.Linq;
using UnityEngine;

namespace uwu.Generic
{
	public class ScriptableDatabase<E> : ScriptableObject where E : ScriptableObject
	{
		[SerializeField] E[] m_items = { };

		public E[] items => m_items;

		public bool Add(E item)
		{
			if (Contains(item)) return false;

			var temp = m_items.ToList();
			temp.Add(item);

			m_items = temp.ToArray();
			return true;
		}

		public bool Remove(E item)
		{
			if (!Contains(item)) return false;

			var temp = m_items.ToList();
			temp.Remove(item);

			m_items = temp.ToArray();
			return true;
		}

		public virtual bool Contains(E item)
		{
			if (item == null || m_items == null)
				return false;

			return m_items.Contains(item);
		}

		public E FetchItem(int index)
		{
			if (m_items.Length == 0)
				return null;

			E item = null;
			var i = (int) Mathf.Repeat(index, m_items.Length);
			item = m_items[i];

			return item;
		}

		public E FetchRandomItem()
		{
			if (m_items.Length == 0)
				return null;

			return items[Random.Range(0, items.Length)];
		}

		public int GetItemIndex(E item)
		{
			var index = -1;
			for (var i = 0; i < items.Length; i++)
				if (items[i] == item) {
					index = i;
					break;
				}

			return index;
		}
	}
}