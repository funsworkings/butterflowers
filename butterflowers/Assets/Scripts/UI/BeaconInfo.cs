using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Wizard;
using UnityEngine.UI;

public class BeaconInfo : MonoBehaviour
{
    Beacon m_beacon = null;
    public Beacon beacon {
        get {
            return m_beacon;
        }
        set
        {
            m_beacon = value;

            string file = "";
            if (value != null)
                file = (beacon.fileEntry == null) ? beacon.file : beacon.fileEntry.ShortName;

            text.text = file;

        }
    }

    CanvasGroup canvasGroup;

    [SerializeField] TMPro.TMP_Text text;
    [SerializeField] GameObject icon;
    [SerializeField] Image icon_image;


    [SerializeField] Sprite iconLearn, iconAction, iconComfort, iconUnknown;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        canvasGroup.alpha = 1f;

        var status = beacon.status;
        var pass = true;

        if (status == Beacon.Status.NULL || status == Beacon.Status.UNKNOWN) {
            icon_image.sprite = iconLearn;
            pass = beacon.learning;
        }
        else if (status == Beacon.Status.COMFORTABLE) {
            icon_image.sprite = iconComfort;
        }
        else if (status == Beacon.Status.ACTIONABLE) {
            icon_image.sprite = iconAction;
        }

        icon.SetActive(pass);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        icon.SetActive(false);
    }
}
