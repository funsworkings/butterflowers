using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

namespace Settings {

    public class Local : Singleton<Local>
    {
        string path = "settings.json";

        [SerializeField] Data localData;
        [SerializeField] FloatArray globalSettings;

        private void Awake() {
            localData = new Data();

            /*path = Path.Combine(Application.persistentDataPath, path);
            localData = DataHandler.ReadJSON<Data>(path);

            if(localData == null)
                localData = new Data();*/
        }

        void OnDestroy() {
            //if(localData != null)
            //    DataHandler.WriteJSON<Data>(localData, path, true);
        }

        #region Sound

            public bool sfx {
                get{ return localData.sfx; }
            }
            public bool SFX {
                get{
                    return localData.sfx;
                }
                set{
                    localData.sfx = value;
                }
            }

            public bool music {
                get{ return localData.music; }
            }
            public bool Music {
                get{
                    return localData.music;
                }
                set{
                    localData.music = value;
                }
            }

        #endregion
        
        #region Accessibility

            public float sensitivity {
                get{ return localData.sensitivity; }
            }
            public float Sensitivity {
                get{
                    return sensitivity;

                    if(globalSettings != null){
                        float min = globalSettings.GetValueFromKey("sensitivity_min");
                        float max = globalSettings.GetValueFromKey("sensitivity_max");
                        float sensitive = sensitivity;
                        return sensitive.Remap(min, max);
                    }
                    else
                        return sensitivity;
                }
                set{
                    localData.sensitivity = Mathf.Clamp01(value);
                }
            }

            public float brightness {
                get{ return localData.brightness; }
            }
            public float Brightness {
                get{
                    return brightness;

                    if (globalSettings != null){
                        float min = globalSettings.GetValueFromKey("brightness_min");
                        float max = globalSettings.GetValueFromKey("brightness_max");
                        float bright = brightness;
                        return bright.Remap(min, max);
                    }
                    else {
                        Debug.Log("Global settings is NULL!");
                        return brightness;
                    }
                }
                set{
                    localData.brightness = Mathf.Clamp01(value);
                }
            }

            public float textSize {
                get { return localData.textSize; }
            }
            public float TextSize {
                get{
                    return textSize;

                    if (globalSettings != null){
                        float min = globalSettings.GetValueFromKey("textsize_min");
                        float max = globalSettings.GetValueFromKey("textsize_max");
                        float size = textSize;
                        return textSize.Remap(min, max);
                    }
                    else
                        return textSize;
                }
                set{
                    localData.textSize = Mathf.Clamp01(value);
                }
            }

        #endregion


        public bool notifications
        {
            get { return localData.notifications; }
        }
        public bool Notifications
        {
            get
            {
                return localData.notifications;
            }
            set
            {
                localData.notifications = value;
            }
        }
    }

}