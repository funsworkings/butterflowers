using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard {

	[CreateAssetMenu(fileName = "New Wizard Memory", menuName = "Internal/Wizard/Memory", order = 52)]
	public class Memory : ScriptableObject {

		public new string name 
		{
			get
			{
				return (image == null) ? "" : image.name;
			}
		}

		public string thumbnail;
		public Texture2D image;
	}

}
