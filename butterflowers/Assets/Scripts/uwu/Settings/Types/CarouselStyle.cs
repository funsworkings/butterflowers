using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings {

    [CreateAssetMenu(fileName = "New Carousel Style", menuName = "Settings/Global/Carousel", order = 52)]
    public class CarouselStyle : Global<FloatSetting, float> {
        public bool infiniteScroll = false;
        public bool clamped        = false;
        public bool scaling        = false;

        public AnimationCurve scaleCurve;
    }

}
