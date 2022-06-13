using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace butterflowersOS.UI
{
	public class MenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		// Properties

		[SerializeField] TMP_Text textField = null;
		[SerializeField] TMP_Text astriskField = null;

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