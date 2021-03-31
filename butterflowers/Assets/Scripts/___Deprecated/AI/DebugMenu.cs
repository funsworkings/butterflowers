using System;
using butterflowersOS.Data;
using TMPro;
using UnityEngine;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.AI
{
	public class DebugMenu : MonoBehaviour
	{
		// Properties

		ToggleOpacity _opacity;

		[SerializeField] TMP_Text logUI, nestFillUI, hobUI;

		// Attributes

		[SerializeField] bool visible = false;
		[SerializeField] int lastLogIndex = -1;
		[SerializeField] string log = "";

		void Awake()
		{
			_opacity = GetComponent<ToggleOpacity>();
		}

		void Start()
		{
			logUI.text = log = ""; // Clear log
			
			ToggleOpacity();
		}

		void Update()
		{
			ToggleOpacity();
		}

		void ToggleOpacity()
		{
			if(visible) _opacity.Show();
			else _opacity.Hide();
		}

		public void PrintLog(SurveillanceLogData _log, int index)
		{
			if (index < lastLogIndex) log = "";
			log += string.Format("\n\nlog {0}:\n{1}", index, JsonUtility.ToJson(_log, true));

			logUI.text = log;
			lastLogIndex = index;
		}
		
		#region Events
		
		
		#endregion


		#region Stats

		public void SetNestFill(float fill)
		{
			if (visible) nestFillUI.text = Mathf.FloorToInt(fill * 100f) + "%";
		}
		
		public void SetHOB(float fill)
		{
			if (visible) hobUI.text = Mathf.FloorToInt(fill * 100f) + "%";
		}

		#endregion
	}
}