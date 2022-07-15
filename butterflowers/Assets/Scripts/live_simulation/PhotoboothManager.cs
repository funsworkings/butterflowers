using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace live_simulation
{
    public class PhotoboothManager : MonoBehaviour, IBridgeUtilListener
    {
        public BridgeUtil _Util { get; set; } = null;
        public Action<float, float> OnBeat { get; } = null;

        [SerializeField] private RawImage _imageView;
        private Material _primaryMat;

        [SerializeField] private PhotoboothBounds _photoboothBounds;
        [SerializeField] public PhotoboothPreset setting;

        void Start()
        {
            if (_imageView)
            {
                _primaryMat = _imageView.material;
            }

            ApplySetting(GenerateRandom());
        }

        public void ApplySetting(PhotoboothPreset _setting)
        {
            setting = _setting;
            
            // Apply values
            _primaryMat.SetFloat("_Blur", 0f);//_primaryMat.SetFloat("_Blur", _setting.blur);
            _primaryMat.SetFloat("_Distortion", _setting.distort);
            _primaryMat.SetFloat("_Brightness", _setting.brightness);
            _primaryMat.SetFloat("_StarVisibility", _setting.star_vis);
            _primaryMat.SetFloat("_StarSpeed", _setting.star_speed);
            _primaryMat.SetFloat("_StarTiling", _setting.star_tile);
        }

        PhotoboothPreset GenerateDefault()
        {
            return new PhotoboothPreset()
            {
                blur = _photoboothBounds.blurMin,
                distort = _photoboothBounds.distortMin,
                brightness = _photoboothBounds.brightnessMin,
                star_vis = _photoboothBounds.starVisMin,
                star_speed = _photoboothBounds.starSpeedMin,
                star_tile = _photoboothBounds.starTileMin
            };
        }

        public PhotoboothPreset GenerateRandom()
        {
            float blur = Random.Range(_photoboothBounds.blurMin, _photoboothBounds.blurMax);
            float distort = Random.Range(_photoboothBounds.distortMin, _photoboothBounds.distortMax);
            float brightness = Random.Range(_photoboothBounds.brightnessMin, _photoboothBounds.brightnessMax);
            float star_vis = Random.Range(_photoboothBounds.starVisMin, _photoboothBounds.starVisMax);
            float star_speed = Random.Range(_photoboothBounds.starSpeedMin, _photoboothBounds.starSpeedMax);
            float star_tiling = Random.Range(_photoboothBounds.starTileMin, _photoboothBounds.starTileMax);

            return new PhotoboothPreset()
            {
                blur = blur,
                distort = distort,
                brightness = brightness,
                star_vis = star_vis,
                star_speed = star_speed,
                star_tile = star_tiling
            };
        }
    }
}