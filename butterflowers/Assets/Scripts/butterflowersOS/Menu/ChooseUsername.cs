using System;
using UnityEngine;
using UnityEngine.Events;
using uwu;
using uwu.Extensions;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Menu
{
	public class ChooseUsername : GenericMenu
	{
		new Camera camera;
		
		// Events

		[System.Serializable] public class UsernameEvent : UnityEvent<string>{}
		public UsernameEvent onValidateInput;

		// Properties

		ToggleOpacity opacity;

		[SerializeField] MainMenu _mainMenu = null;
		[SerializeField] CanvasGroup usernameContainer = null;
		[SerializeField] TMPro.TMP_InputField inputfield = null;
		[SerializeField] RectTransform inputrect = null;
		
		[SerializeField] ToggleOpacity submitButtonOpacity = null;
		[SerializeField] GameObject pr_burst = null;

		bool isInputValid = false;

		// Attributes

		[SerializeField] string input;

		bool isActive = false;
		
		#region Monobehaviour callbacks

		void Awake()
		{
			opacity = GetComponent<ToggleOpacity>();
		}

		protected override void Start()
		{
			camera = Camera.main;
			base.Start();
		}

		void Update()
		{
			if (!IsVisible) return;
			
			if(!Input.GetKeyUp(KeyCode.Escape))
			{
				isActive = usernameContainer.interactable;
				if (isActive && !inputfield.isFocused)
					onFocusInput();

				ConfigureInputField();
				
				if (Input.GetKeyUp(KeyCode.Return) && isInputValid) 
				{
					SubmitInput();
				}

				return;
			}
			
			_mainMenu.Reset();
		}

		#endregion
		
		#region Menu
		
		protected override void DidOpen()
		{
			opacity.Show();
			isInputValid = false;
			
			inputfield.onValueChanged.AddListener(onEditInput);
		}

		protected override void DidClose()
		{
			inputfield.onValueChanged.RemoveListener(onEditInput);
			
			inputfield.text = ""; // Wipe input
			opacity.Hide();
			isInputValid = false;
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

			isInputValid = !string.IsNullOrEmpty(value);
			if (!isInputValid)
				submitButtonOpacity.Hide();
			else
				submitButtonOpacity.Show();
		}

		public void SubmitInput()
		{
			onValidateInput.Invoke(input);
		}
		
		#endregion

		void Burst(Vector2 screen_pos)
		{
			Vector3 pos = camera.ScreenToWorldPoint(new Vector3(screen_pos.x, screen_pos.y, 10f));
			Instantiate(pr_burst, pos, pr_burst.transform.rotation);
		}
	}
}