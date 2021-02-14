using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Objects.Entities.Interactables;
using UnityEngine;

using Wizard;
using UnityEngine.UI;
using uwu.Extensions;

[Obsolete("Obsolete API!", true)]
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
        
        return file;
    }

    public string parseBeaconFileName(Beacon beacon)
    {
        //return (beacon.fileEntry == null) ? beacon.File : beacon.fileEntry.ShortName;
        return null;
    }
}
