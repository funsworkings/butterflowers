using butterflowersOS.Objects.Miscellaneous;
using UnityEngine;
using uwu.Extensions;

namespace butterflowersOS.Data
{
	[System.Serializable]
	public class LeafData
	{
		public byte index = 0;
		
		public byte interval = 0;
		public byte rotation = 0;

		public LeafData(Leaf leaf)
		{
			this.index = (byte)leaf.index;
			this.interval = (byte)Mathf.RoundToInt(leaf.progress * 255f);
			this.rotation = (byte)Mathf.FloorToInt((leaf.offset / 360f) * 255f);
		}
	}
}