using System.Collections.Generic;
using UnityEngine;

namespace uwu.Snippets
{
	public class Logger : MonoBehaviour
	{
		#region Collections

		protected List<string> messages = new List<string>();

		#endregion
		
		#region Attributes

		[SerializeField] int capacity = 10;
		[SerializeField] protected bool duplicates = true;
		[SerializeField] protected bool auto, resize;

		#endregion

		public void Push(string message)
		{
			var duplicate = messages.Contains(message);
			if (!duplicates)
				return;

			if (resize) capacity = messages.Count + 1;

			var overflow = messages.Count + 1 > capacity;
			if (overflow) {
				if (auto) Pop();
				else {
					Debug.LogWarning("Attempt to add element to exceed capacity of fixed logger, ignore..");
					return;
				}
			}

			messages.Add(message);
			onPushElement(messages.Count-1, message);
		}

		void Pop()
		{
			if (messages.Count > 0)
				Remove(0);
		}

		public void Remove(int index)
		{
			if (index >= 0 && index <= messages.Count - 1) {
				var message = messages[index];
				messages.RemoveAt(index);
				onPopElement(index, message);
			}
		}

		public void Clear()
		{
			var allItems = messages.ToArray();
			for (var i = 0; i < allItems.Length; i++)
				Remove(0);
		}

		#region Callbacks

		protected virtual void onPushElement(int index, string message)
		{
		}

		protected virtual void onPopElement(int index, string message)
		{
		}

		#endregion
	}
}