using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings {

    [CreateAssetMenu(fileName = "New Float Array", menuName = "Settings/Global/Strings", order = 52)]
    public class StringArray : Global<StringSetting, string> {}

}