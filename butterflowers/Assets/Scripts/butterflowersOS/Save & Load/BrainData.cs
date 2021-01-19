using System;
using System.Globalization;
using Neue.Agent.Brain.Data;
using UnityEngine;
using uwu;

namespace butterflowersOS
{
	[System.Serializable]
	public class BrainData
	{
		public string username = null;
		public string created_at = null;

		public string[] directories = new string[]{};
		public string[] files = new string[] { };
		
		public int[] user_files = new int[] { };
		public int[] shared_files = new int[] { };
		public int[] world_files = new int[] { };

		public Profile profile = new Profile();
		
		public byte[] images = new byte[]{};

		public BrainData(){}

		public BrainData(GameData dat, byte[] images)
		{
			this.username = dat.username;
			this.created_at = System.DateTime.UtcNow.ToString(); // Write timestamp
			
			this.directories = dat.directories;
			this.files = dat.files;
			this.user_files = dat.user_files;
			this.shared_files = dat.shared_files;
			this.world_files = dat.world_files;

			this.profile = dat.profile;
			this.images = images;
		}
	}

	public static class BrainDataExtensions
	{
		public static bool IsProfileValid(this BrainData data)
		{
			if (data == null) return false;
			
			var timestamp = data.created_at;
			try 
			{
				DateTime created_at = DateTime.Parse(timestamp, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
				
				Debug.LogWarningFormat("Profile in save file was valid => created at: {0}", created_at);
				return true;
			}
			catch (System.Exception err) 
			{
				Debug.LogWarning("Profile in save file was invalid!");
				return false;	
			}
		}
	}
}