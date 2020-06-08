using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard {

    public class Brain: MonoBehaviour {

		#region Properties

		Controller controller;
		Memories memories;

		#endregion

		#region Collections

		List<Memory> temp = new List<Memory>();

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			controller = GetComponent<Controller>();
		}

		void Start()
		{
			memories = controller.Memories;
		}

		void OnDestroy()
		{
			Dispose();
		}

		#endregion

		#region Operations

		public void Dispose()
		{
			for (int i = 0; i < temp.Count; i++) {
				var mem = temp[i];
				if (mem != null) memories.Remove(mem);
			}
		}

		public void AddMemory(Texture2D img)
		{
			var mem = memories.AddMemory(img);
			if (mem != null) temp.Add(mem);
		}

		#endregion
	}

}