using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using uwu;
using uwu.UI.Behaviors.Visibility;

namespace Intro
{
	public class ChooseUsername : MonoBehaviour
	{
		// Events

		public UnityEvent onValidateInput;
		
		// External

		GameDataSaveSystem Save;
		
		// Properties

		[SerializeField] TMPro.TMP_InputField usernameInput;
		[SerializeField] ToggleOpacity submitButtonOpacity;
		
		// Attributes

		[SerializeField] string input;
		
		#region Monobehaviour callbacks

		void OnEnable()
		{
			Save = GameDataSaveSystem.Instance;
			
			usernameInput.onValueChanged.AddListener(onEditInput);
		}

		void OnDisable()
		{
			usernameInput.onValueChanged.RemoveListener(onEditInput);
		}
		
		#endregion
		
		#region Input

		void onEditInput(string value)
		{
			input = value;

			if (string.IsNullOrEmpty(value))
				submitButtonOpacity.Hide();
			else
				submitButtonOpacity.Show();
		}

		public void SubmitInput()
		{
			Save.username = input;
			onValidateInput.Invoke();
		}
		
		#endregion
	}
}