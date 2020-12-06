using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Wizard;
using UnityEngine.UI;
using uwu.Extensions;

public class BeaconInfo : MonoBehaviour
{
    string template = "{0}\n\n<color={1}>[ A ] dd to nest\n[ P ] lant in terrain"; 

    [SerializeField] TMPro.TMP_Text text;
    [SerializeField] Color unknownColor, knownColor;

    public string parseTextFromBeacon(Beacon beacon)
    {
        Color color = unknownColor;
        string color_hex = Extensions.ParseColor(color);

        string file = "";
        if (beacon != null)
            file = parseBeaconFileName(beacon);

        var def_file = file;
        return file;
        
        return string.Format(template, def_file, color_hex);
    }

    public string parseBeaconFileName(Beacon beacon)
    {
        return (beacon.fileEntry == null) ? beacon.file : beacon.fileEntry.ShortName;
    }
}
