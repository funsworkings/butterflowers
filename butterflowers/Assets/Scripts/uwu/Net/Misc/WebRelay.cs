using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Net;

public class WebRelay : MonoBehaviour
{
    [SerializeField] string url = "URL_GOES_HERE";

    public void GoTo()
    {
        WebHandler.Instance.GoTo(url);
    }

    public void GoTo(string url)
    {
        WebHandler.Instance.GoTo(url);
    }
}
