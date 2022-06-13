using System;
using UnityEngine;

namespace butterflowersOS.Objects.Miscellaneous
{
	public class MouseLookCapture : MonoBehaviour
	{
		public Vector3 pivotRot0 { get; set; } = Vector3.zero;
		public Vector3 pivotRot1 { get; set; } = Vector3.zero;
		
		MouseLook[] looks = new MouseLook[]{};

		void Start()
		{
			looks = GetComponentsInChildren<MouseLook>();
		}

		void Update()
		{
			Read();
		}

		void Read()
		{
			pivotRot0 = looks[0].transform.localEulerAngles;
			pivotRot1 = looks[1].transform.localEulerAngles;
		}

		void Write()
		{
			looks[0].transform.localEulerAngles = pivotRot0;
			looks[1].transform.localEulerAngles = pivotRot1;
		}
	}
}