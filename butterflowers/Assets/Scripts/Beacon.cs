using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;

public class Beacon : MonoBehaviour
{
    Navigator navigator;

    [SerializeField] FileSystemEntry file = null;

    void Awake() {
        navigator = FindObjectOfType<Navigator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        file = navigator.GetRandomFileInDirectory(type: Navigator.TypeFilter.Normal);
    }

    void OnMouseDown()
    {
        Debug.Log("loaded:" + file.Path);
        navigator.Refresh(file); // Load file from beacon
    }
}
