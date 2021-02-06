using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace butterflowersOS.UI
{
	public class MenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		string defaultFormat = "{0}";
		string hoverFormat = "{0}  *";
		
		// Properties

		[SerializeField] TMP_Text textField;
		[SerializeField] TMP_Text astriskField;

		public string DefaultText { get; set; }

		void Awake()
		{
			DefaultText = textField.text;
		}

		void Start()
		{
			UpdateText(false);
		}

		void Update()
		{
			textField.text = DefaultText;
		}

		void UpdateText(bool hover)
		{
//			string format = (hover) ? hoverFormat : defaultFormat;
//			textField.text = string.Format(format, DefaultText);

			astriskField.enabled = hover;
		}

		#region Pointer events
		
		public void OnPointerEnter(PointerEventData eventData)
		{
			UpdateText(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			UpdateText(false);
		}
		
		#endregion
	}
}