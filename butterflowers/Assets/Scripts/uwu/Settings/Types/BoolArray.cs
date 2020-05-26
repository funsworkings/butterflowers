using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings {

    [CreateAssetMenu(fileName = "New Float Array", menuName = "Settings/Global/Bools", order = 52)]
    public class BoolArray : Global<BoolSetting, bool> {}

}