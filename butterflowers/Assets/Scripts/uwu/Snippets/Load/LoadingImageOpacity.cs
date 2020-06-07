using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

[RequireComponent(typeof(Loading))]
public class LoadingImageOpacity: MonoBehaviour {
    Loading loading;
    [SerializeField] Image fill;

    void Awake()
    {
        loading = GetComponent<Loading>();
    }

    void OnEnable()
    {
        loading.onProgress += UpdateFill;
    }

    void OnDisable()
    {
        loading.onProgress -= UpdateFill;
    }

    void UpdateFill(float value)
    {
        fill.color = Extensions.SetOpacity(value, fill.color);
    }
}
