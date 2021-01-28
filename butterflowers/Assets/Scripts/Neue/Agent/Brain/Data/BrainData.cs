using System;
using System.Globalization;
using butterflowersOS.Data;
using uwu;

namespace Neue.Agent.Brain.Data
{
	[System.Serializable]
	public class BrainData
	{
		public string username = null; //
		public string created_at = null; //
		
		public SurveillanceData[] surveillanceData = new SurveillanceData[]{};

		public string[] directories = new string[]{};
		public string[] files = new string[] { };
		
		public ushort[] user_files = new ushort[] { };
		public ushort[] shared_files = new ushort[] { };
		public ushort[] world_files = new ushort[] { };

		public Profile profile = new Profile(); //
		public byte[] images = new byte[]{}; //
		public ushort image_height = 0;

		public BrainData(){}

		public BrainData(GameData dat, byte[] images, ushort image_height)
		{
			this.username = dat.username;
			this.created_at = System.DateTime.UtcNow.ToString(); // Write timestamp

			this.surveillanceData = dat.surveillanceData;
			
			this.directories = dat.directories;
			this.files = dat.files;
			this.user_files = dat.user_files;
			this.shared_files = dat.shared_files;
			this.world_files = dat.world_files;

			this.profile = dat.profile;
			this.images = images;
			this.image_height = image_height;
		}
	}

	public static class BrainDataExtensions
	{
		public static bool IsProfileValid(this BrainData data)
		{
			if (data == null) return false;
			
			var created_at = data.created_at;
			return IsProfileTimestampValid(created_at);
		}
		
		public static bool IsProfileTimestampValid(string created_at)
		{
			try 
			{
				DateTime _created_at = DateTime.Parse(created_at, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
				
				UnityEngine.Debug.LogWarningFormat("Profile in save file was valid => created at: {0}", _created_at);
				return true;
			}
			catch (System.Exception err) 
			{
				UnityEngine.Debug.LogWarning("Profile in save file was invalid!");
				return false;	
			}
		}
	}
}