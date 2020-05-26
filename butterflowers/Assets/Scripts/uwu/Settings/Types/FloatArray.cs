using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings {

    [CreateAssetMenu(fileName = "New Float Array", menuName = "Settings/Global/Floats", order = 52)]
    public class FloatArray : Global<FloatSetting, float> {}

}