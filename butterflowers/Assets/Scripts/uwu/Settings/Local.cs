using UnityEngine;
using uwu.Extensions;
using uwu.Settings.Types;

namespace uwu.Settings
{
	public class Local : Singleton<Local>
	{
		[SerializeField] Data localData;
		[SerializeField] FloatArray globalSettings;
		string path = "settings.json";


		public bool notifications => localData.notifications;

		public bool Notifications
		{
			get => localData.notifications;
			set => localData.notifications = value;
		}

		void Awake()
		{
			localData = new Data();

			/*path = Path.Combine(Application.persistentDataPath, path);
			localData = DataHandler.ReadJSON<Data>(path);

			if(localData == null)
			    localData = new Data();*/
		}

		void OnDestroy()
		{
			//if(localData != null)
			//    DataHandler.WriteJSON<Data>(localData, path, true);
		}

		#region Sound

		public bool sfx => localData.sfx;

		public bool SFX
		{
			get => localData.sfx;
			set => localData.sfx = value;
		}

		public bool music => localData.music;

		public bool Music
		{
			get => localData.music;
			set => localData.music = value;
		}

		#endregion

		#region Accessibility

		public float sensitivity => localData.sensitivity;

		public float Sensitivity
		{
			get
			{
				return sensitivity;

				if (globalSettings != null) {
					var min = globalSettings.GetValueFromKey("sensitivity_min");
					var max = globalSettings.GetValueFromKey("sensitivity_max");
					var sensitive = sensitivity;
					return sensitive.Remap(min, max);
				}
				else {
					return sensitivity;
				}
			}
			set => localData.sensitivity = Mathf.Clamp01(value);
		}

		public float brightness => localData.brightness;

		public float Brightness
		{
			get
			{
				return brightness;

				if (globalSettings != null) {
					var min = globalSettings.GetValueFromKey("brightness_min");
					var max = globalSettings.GetValueFromKey("brightness_max");
					var bright = brightness;
					return bright.Remap(min, max);
				}
				else {
					Debug.Log("Global settings is NULL!");
					return brightness;
				}
			}
			set => localData.brightness = Mathf.Clamp01(value);
		}

		public float textSize => localData.textSize;

		public float TextSize
		{
			get
			{
				return textSize;

				if (globalSettings != null) {
					var min = globalSettings.GetValueFromKey("textsize_min");
					var max = globalSettings.GetValueFromKey("textsize_max");
					var size = textSize;
					return textSize.Remap(min, max);
				}
				else {
					return textSize;
				}
			}
			set => localData.textSize = Mathf.Clamp01(value);
		}

		#endregion
	}
}