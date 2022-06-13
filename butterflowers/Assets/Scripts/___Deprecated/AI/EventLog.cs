using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace butterflowersOS.AI
{
	public class EventLog : MonoBehaviour
	{
		// Properties

		[SerializeField] TMP_Text textField;

		bool read = false;
		
		// Attributes

		[SerializeField] float stackPopDelay = .1f;
		[SerializeField] int stackHeight = 10;
		
		// Collections
		
		List<string> eventcodes = new List<string>();
		List<string> eventqueue = new List<string>();
		

		void Awake()
		{
			if (textField == null) textField = GetComponent<TMP_Text>();
		}

		void Start()
		{
			ClearEventStack();
		}

		#region Stack

		public void Push(EVENTCODE[] @events)
		{
			if (!read) 
			{
				StartCoroutine("Parse", @events);
				read = true;
			}
			else 
			{
				Read(eventqueue); // Immediately pop all elements
				
				StopCoroutine("Parse");
				StartCoroutine("Parse", @events);
			}
		}

		IEnumerator Parse(EVENTCODE[] elements)
		{
			ClearEventStack();
		
			eventqueue = elements.Select(_e => _e.ToString()).ToList();

			string[] eventstack = eventqueue.ToArray();
			foreach (string e in eventstack) 
			{
				Read(new string[] { e });
				eventqueue.RemoveAt(0);
				yield return new WaitForSeconds(stackPopDelay);
			}

			read = false;
		}

		void Read(IEnumerable<string> elements)
		{
			for (int i = 0; i < elements.Count(); i++) 
			{
				string el = elements.ElementAt(i);
				if (!string.IsNullOrEmpty(el)) 
				{
					if(eventcodes.Count > stackHeight) eventcodes.RemoveAt(0);
					eventcodes.Add(el);
					
					DisplayEventStack();
				}
			}
		}

		#endregion
		
		#region UI

		void DisplayEventStack()
		{
			textField.text = string.Join("\n", eventcodes);
		}

		void ClearEventStack()
		{
			textField.text = "";
			eventcodes = new List<string>();
		}
		
		#endregion
	}
}