using UnityEngine;
using uwu.UI.Behaviors.Visibility;

namespace uwu.UI.Elements
{
	public class Category : MonoBehaviour
	{
		[SerializeField] TogglePosition options;
		ToggleOpacity opacity;

		void Awake()
		{
			opacity = GetComponent<ToggleOpacity>();
		}

		public void Show()
		{
			opacity.Show();
			options.Show();
		}

		public void Hide()
		{
			opacity.Hide();
			options.Hide();
		}
	}
}