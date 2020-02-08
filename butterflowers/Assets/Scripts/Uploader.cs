using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;


public class Uploader : MonoBehaviour
{
    public delegate void OnSuccessReceiveImage(byte[] data);
    public static event OnSuccessReceiveImage onSuccessReceiveImage;

    public delegate void OnFailReceiveImage();
    public static event OnFailReceiveImage onFailReceiveImage;
        
    // Start is called before the first frame update
    void Start()
    {
        FileBrowser.SetFilters(false, new string[] { ".jpg", ".png" });
    }

    // Update is called once per frame
    void Update()
    {
        if(!FileBrowser.IsOpen){
            if(Input.GetKeyDown(KeyCode.U))
                FileBrowser.ShowLoadDialog(OnSuccess, OnCancel);
        }
    }   

    void OnSuccess(string path){
        Debug.Log("Successfully loaded file at path: " + path);

        if(onSuccessReceiveImage != null) {
            var bytes = FileBrowserHelpers.ReadBytesFromFile( FileBrowser.Result );
            onSuccessReceiveImage(bytes);
        }
    }

    void OnCancel(){

    }
}
