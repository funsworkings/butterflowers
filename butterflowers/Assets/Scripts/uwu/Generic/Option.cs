using UnityEngine;

namespace uwu.Generic
{
	/// <summary>
	///     Generic class for options within a selection, send select events to selection
	/// </summary>
	public class Option<T> : MonoBehaviour
	{
		public delegate void Selected(Option<T> option);

		[SerializeField] protected string id;

		public int index;

		protected bool active; // Currently selected?

		[SerializeField] protected T attribute; // Data contained within option
		protected Selection<T> selection;

		public string Id => id;

		public virtual T Attribute
		{
			get => attribute;
			set => attribute = value;
		}

		public static event Selected onSelected;

		public void Bind(Selection<T> selection)
		{
			this.selection = selection; // Assign selecto to communicate with
		}

		public virtual void Select()
		{
			//if(active) return;

			active = true;
			if (selection != null)
				selection.SelectOption(this);

			if (onSelected != null)
				onSelected(this);
		}

		public virtual void Deselect()
		{
			if (!active) return;

			active = false;
		}
	}
}