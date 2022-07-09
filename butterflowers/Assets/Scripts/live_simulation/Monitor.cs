using System;
using System.Collections;
using System.IO;
using live_simulation.Utils;
using UnityEngine;
using UnityEngine.UI;
using uwu.IO;

namespace live_simulation
{
    public class Monitor : MonoBehaviour
    {
        // Properties
    
        [SerializeField] Webcam _webcam;
        [SerializeField] RawImage _webcamTargetImage, _webcamTargetLiveImage;
        private AspectRatioFitter _webcamTargetImageFitter;

        private bool _wait = false;

        // Attributes

        [SerializeField, Range(0f, 1f)] private float _captureWResolution, _captureHResolution;
        [SerializeField] private float _updateInterval = 1f;
        
        
        #region Files
        
        string savePath
        {
            get
            {
                var path = Application.persistentDataPath + "/scratch_disk";
                FileUtils.EnsureDirectory(path);
                return path;
            }
        }

        void ClearDisk()
        {
            string folder = savePath;
            string[] items = Directory.GetFiles(folder);
            if (items != null && items.Length > 0)
            {
                foreach (string s in items)
                {
                    File.Delete(s); // Delete file at path
                }
            }
        }

        void SaveToDisk(Texture2D img)
        {
            var bytes = img.EncodeToJPG();
            var filename = System.Guid.NewGuid().ToString() + ".jpg";
            var fullpath = savePath + $"/{filename}";
            
            File.WriteAllBytes(fullpath, bytes);
            Debug.LogWarning($"Success save image to path? {fullpath} -- {File.Exists(fullpath)}");
            
            BridgeUtil.onCreateImage?.Invoke(fullpath);
        }
        
        
        #endregion

        IEnumerator Start()
        {
            _webcamTargetImageFitter = _webcamTargetImage.GetComponent<AspectRatioFitter>();

            yield return new WaitForEndOfFrame();
            ClearDisk();
            
            while (!_webcam.Ready) yield return null;
            CaptureWebcamImage();
            
            BridgeUtil.onCameraChange += SwitchWebcamDevice;

            //StartCoroutine(Loop());
        }

        IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(_updateInterval);
                while (_wait) yield return null;
                SwitchWebcamDevice(); // Jump to next webcam
            }
        }

        void Update()
        {
            if(Input.GetKeyUp(KeyCode.Space)) CaptureWebcamImage();
            else if(Input.GetKeyUp(KeyCode.RightArrow)) SwitchWebcamDevice();

            var tex = _webcam.CurrentActiveRenderTarget;
            _webcamTargetLiveImage.texture = tex;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            
            ClearDisk(); // Wipe scratch disk

            BridgeUtil.onCameraChange -= SwitchWebcamDevice;
        }

        void CaptureWebcamImage()
        {
            if (_wait) return;
            _wait = true;
            
            var w = Mathf.FloorToInt(Screen.width * _captureWResolution);
            var h = Mathf.FloorToInt(Screen.height * _captureHResolution);
            
            _webcam.Capture(w, h, result =>
            {
                _webcamTargetImage.texture = result;
                _wait = false;

                if (result != null)
                {
                    SaveToDisk(result);
                }
                //_webcamTargetImageFitter.aspectRatio = (result != null)? 1f * result.width / result.height:1f;
            });
        }

        void SwitchWebcamDevice()
        {
            if (_wait) return;
            _wait = true;
            
            _webcam.RequestNextDevice(success =>
            {
                Debug.LogWarning("Switch to next webcam was successful!");
                _wait = false;
            });
        }
    }
}