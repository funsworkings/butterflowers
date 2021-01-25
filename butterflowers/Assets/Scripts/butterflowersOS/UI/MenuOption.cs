using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace butterflowersOS.UI
{
	public class MenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		string defaultFormat = "{0}";
		string hoverFormat = "- {0} -";
		
		// Properties

		TMP_Text textField;
		string defaultText;


		void Awake()
		{
			textField = GetComponentInChildren<TMP_Text>();
		}

		void Start()
		{
			defaultText = textField.text;
			UpdateText(false);
		}

		void UpdateText(bool hover)
		{
			string format = (hover) ? hoverFormat : defaultFormat;
			textField.text = string.Format(format, defaultText);
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