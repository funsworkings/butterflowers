using butterflowersOS.Objects.Miscellaneous;

namespace butterflowersOS.Data
{
	[System.Serializable]
	public class LeafData
	{
		public int index = -1;
		public float interval = 0f;
		public float scale = 1f;
		public float rotation = 0f;

		public LeafData(Leaf leaf)
		{
			this.index = leaf.index;
			this.interval = leaf.progress;
			this.scale = leaf.Scale;
			this.rotation = leaf.offset;
		}
	}
}