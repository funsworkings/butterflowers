namespace butterflowersOS.Data
{
	[System.Serializable]
	public class LibraryPayload
	{
		public string[] directories = new string[]{};
		public string[] files = new string[]{};

		public int[] userFiles = new int[] { };
		public int[] sharedFiles = new int[]{};
		public int[] worldFiles = new int[] { };
	}
}