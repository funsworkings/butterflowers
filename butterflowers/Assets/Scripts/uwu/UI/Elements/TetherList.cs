using System;
using System.Collections.Generic;
using UnityEngine;
using uwu.Behaviors;

namespace uwu.UI.Elements
{
	public class TetherList<E> : MonoBehaviour
	{
		[SerializeField] GameObject tetherBlockPrefab;

		protected List<GameObject> blocks = new List<GameObject>();
		public Action onPopulateList;
		protected List<ITether<E>> tethers = new List<ITether<E>>();

		protected virtual void OnEnable()
		{
		}

		protected virtual void OnDisable()
		{
		}

		public void PopulateList(E[] elements, Transform root = null, bool events = true)
		{
			if (root == null)
				root = transform;

			if (elements != null)
				foreach (var el in elements)
					if (validateItem(el)) {
						var obj = Instantiate(tetherBlockPrefab, root);
						var tether = obj.GetComponent<ITether<E>>();
						tether.tether = el;

						tethers.Add(tether);
						onPopulateItem(tether);
						blocks.Add(obj);
					}

			if (events && onPopulateList != null)
				onPopulateList();
		}

		protected virtual bool validateItem(E tether)
		{
			return true;
		}

		protected virtual void onPopulateItem(ITether<E> tether)
		{
		}

		public void ClearList()
		{
			var blocks = this.blocks.ToArray();

			foreach (var block in blocks)
				Destroy(block);

			this.blocks = new List<GameObject>();
			tethers = new List<ITether<E>>();
		}
	}
}