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

		[TextArea(2, 30)] public string body = "";
		public string body_formatted {
			get
			{
				return body.Replace(":m:", string.Format(":i:{0}:i:", thumbnail));
			}
		}

		public string thumbnail;
		public Texture2D image;
		public AudioClip audio;

		[Range(-1f, 1f)] public float weight = 0f;
	}

}
