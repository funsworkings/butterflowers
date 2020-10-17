using UnityEngine;
using UnityEngine.Events;

namespace uwu.Generic
{
	/// <summary>
	///     Generic class for selections with multiple options
	/// </summary>
	public class Selection<T> : MonoBehaviour
	{
		public delegate void OnOptionSelected(T option); // Delegate for when option is selected

		public UnityEvent OnSelect; // Global event for when option is selected

		[SerializeField] protected Option<T>[] options;

		[SerializeField] protected Option<T> selected; // Option currently selected
		public OnOptionSelected SelectedOption;

		public Option<T> Selected
		{
			get => selected;

			set => selected = value;
		}

		// Start is called before the first frame update
		protected virtual void Start()
		{
			if (selected != null)
				selected.Select();

			// Attach selection to options
			foreach (var o in options)
				o.Bind(this);
		}

		public void SelectOption(Option<T> option)
		{
			if (selected != null && selected != option) // Deselect previous option if exists and different than current
				selected.Deselect();

			selected = option;

			Debug.Log("selected option: " + option.Id);

			// Fire selection events
			SelectedOption(option.Attribute);
			OnSelect.Invoke();
		}

		public void Clear()
		{
			if (selected != null)
				selected.Deselect(); // Clear selected option

			selected = null;
		}
	}
}