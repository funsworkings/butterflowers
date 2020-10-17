using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class VersionText : MonoBehaviour
{
    TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        text.text = string.Format("version = {0}", Application.version);
    }
}
