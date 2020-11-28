using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using uwu;
using uwu.Extensions;
using uwu.UI.Behaviors.Visibility;

namespace Intro
{
	public class ChooseUsername : MonoBehaviour
	{
		Camera camera;
		
		// Events

		public UnityEvent onValidateInput;
		
		// External

		GameDataSaveSystem Save;
		
		// Properties

		[SerializeField] CanvasGroup usernameContainer;
		[SerializeField] TMPro.TMP_InputField inputfield;
		[SerializeField] RectTransform inputrect;
		
		[SerializeField] ToggleOpacity submitButtonOpacity;
		[SerializeField] GameObject pr_burst;

		[SerializeField] RectTransform debug;
		
		// Attributes

		[SerializeField] string input;

		bool isActive = false;
		
		#region Monobehaviour callbacks

		void OnEnable()
		{
			Save = GameDataSaveSystem.Instance;
			
			inputfield.onValueChanged.AddListener(onEditInput);
		}

		void OnDisable()
		{
			inputfield.onValueChanged.RemoveListener(onEditInput);
		}

		void Start()
		{
			camera = Camera.main;
		}

		void Update()
		{
			isActive = usernameContainer.interactable;
			if(isActive && !inputfield.isFocused)
				onFocusInput();
			
			ConfigureInputField();
		}

		#endregion
		
		#region Input

		public void onFocusInput()
		{
			inputfield.Select();
			inputfield.ActivateInputField();
		}

		void ConfigureInputField()
		{
			if (inputfield.isFocused) 
			{
				inputfield.caretPosition = input.Length;
			}
		}

		void onEditInput(string value)
		{
			input = value;

			var width = inputfield.textComponent.textBounds.extents.x * 2f * GetComponentInParent<Canvas>().scaleFactor;

			var rect = Extensions.RectTransformToScreenSpace(inputrect);
			
			var origin = new Vector2(rect.x + rect.width/2f, rect.y);
			var screen = origin + Vector2.right * width/2f;

			//debug.position = screen;
			
			Burst(screen);

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

		void Burst(Vector2 screen_pos)
		{
			Vector3 pos = camera.ScreenToWorldPoint(new Vector3(screen_pos.x, screen_pos.y, 10f));
			Instantiate(pr_burst, pos, pr_burst.transform.rotation);
		}
	}
}