using System;
using UnityEngine;

namespace uwu
{
	[Serializable]
	public partial class GameData
	{
		string id = null;
		public string ID
		{
			get
			{
				if (id == null) id = System.Guid.NewGuid().ToString();
				return id;
			}
		}
		
		public string BUILD_VERSION = "0.0";
		public string TIMESTAMP = "0";
	}
}