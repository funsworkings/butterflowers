using System;
using System.Collections;
using System.IO;
using live_simulation.Utils;
using UnityEngine;
using UnityEngine.UI;
using uwu.IO;
using Random = Unity.Mathematics.Random;

namespace live_simulation
{
    public class Monitor : MonoBehaviour, IBridgeUtilListener
    {
        // Properties
    
        [SerializeField] Webcam _webcam;
        [SerializeField] RawImage _webcamTargetImage, _webcamTargetLiveImage;
        [SerializeField] private SelectionBox _selection;

        private AspectRatioFitter _webcamTargetImageFitter;

        private bool _wait = false;

        // Attributes

        [SerializeField, Range(0f, 1f)] private float _captureWResolution, _captureHResolution;
        [SerializeField] private float selectionBoxTransitionTime = 1f, selectionBoxWaitTime = 1f;
        
        
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

        string SaveToDisk(Texture2D img)
        {
            var bytes = img.EncodeToJPG();
            var filename = System.Guid.NewGuid().ToString() + ".jpg";
            var fullpath = savePath + $"/{filename}";
            
            File.WriteAllBytes(fullpath, bytes);
            Debug.LogWarning($"Success save image to path? {fullpath} -- {File.Exists(fullpath)}");
            
            BridgeUtil.onCreateImage?.Invoke(fullpath);
            return fullpath;
        }
        
        
        #endregion

        IEnumerator Start()
        {
            _webcamTargetImageFitter = _webcamTargetImage.GetComponent<AspectRatioFitter>();

            yield return new WaitForEndOfFrame();
            ClearDisk();
            
            while (!_webcam.Ready) yield return null;
            //BridgeUtil.onCameraChange += SwitchWebcamDevice;
        }

        void Update()
        {
            if(Input.GetKeyUp(KeyCode.Space)) CaptureWebcamImage(includeSelectionTransition:false);
            else if(Input.GetKeyUp(KeyCode.RightArrow)) SwitchWebcamDevice();

            var tex = _webcam.CurrentActiveRenderTarget;
            _webcamTargetLiveImage.texture = tex;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            
            ClearDisk(); // Wipe scratch disk

            //BridgeUtil.onCameraChange -= SwitchWebcamDevice;
        }

        public void CaptureWebcamImage(System.Action<Texture2D, string> onComplete = null, bool includeSelectionTransition = true)
        {
            if (_wait) throw new SystemException("Cannot capture image while in-progress");
            _wait = true;
            
            var w = Mathf.FloorToInt(Screen.width * _captureWResolution);
            var h = Mathf.FloorToInt(Screen.height * _captureHResolution);
            
            var cx = _selection.X;
            var cy = _selection.Y;
            var cw = _selection.W;
            var ch = _selection.H;

            if (includeSelectionTransition)
            {
                // Change selection zone
                var _cw = _selection.Container.rect.width;
                var _ch = _selection.Container.rect.height;

                Vector2 sPos = new Vector2(UnityEngine.Random.Range(-_cw/2f, _cw/2f), UnityEngine.Random.Range(-_ch/2f, _ch/2f));
                Vector2 sScale = new Vector2(UnityEngine.Random.Range(64, _cw), UnityEngine.Random.Range(64, _ch));
                _selection.UpdateTransform(sPos, sScale, selectionBoxTransitionTime, selectionBoxWaitTime, () =>
                {
                    cx = _selection.X;
                    cy = _selection.Y;
                    cw = _selection.W;
                    ch = _selection.H;

                    _webcam.Capture(w, h, result =>
                    {
                        _webcamTargetImage.texture = result;
                        _wait = false;

                        string fullpath = null;
                        if (result != null)
                        {
                            fullpath = SaveToDisk(result);
                        }
                
                        _webcamTargetImageFitter.aspectRatio = (result != null)? 1f * result.width / result.height:1f;
                        onComplete?.Invoke(result, fullpath);
                
                    }, (int)cw, (int)ch, (int)cx, (int)cy); 
                }, _Util);
            }
            else
            {
                _webcam.Capture(w, h, result =>
                {
                    _webcamTargetImage.texture = result;
                    _wait = false;

                    string fullpath = null;
                    if (result != null)
                    {
                        fullpath = SaveToDisk(result);
                    }
                
                    _webcamTargetImageFitter.aspectRatio = (result != null)? 1f * result.width / result.height:1f;
                    onComplete?.Invoke(result, fullpath);
                
                }, (int)cw, (int)ch, (int)cx, (int)cy); 
            }
        }

        void SwitchWebcamDevice()
        {
            SwitchWebcamDevice(null); // 0 callback
        }

        public void SwitchWebcamDevice(System.Action<WebCamTexture> onComplete = null)
        {
            if (_wait) throw new SystemException("Cannot change device while in-progress");
            _wait = true;

            StartCoroutine(SwitchDeviceRoutine(onComplete));
        }

        IEnumerator SwitchDeviceRoutine(System.Action<WebCamTexture> onComplete)
        {
            var tDiff = (Beat_T - Time.time);
            Debug.LogWarning($"Wait {tDiff}s before trigger webcam!");
            yield return new WaitForSecondsRealtime(tDiff);
            
            _webcam.RequestNextDevice(texture =>
            {
                Debug.LogWarning("Switch to next webcam was successful!");
                _wait = false;
                
                onComplete?.Invoke(texture);
            });
        }

        public BridgeUtil _Util { get; set; } = null;
        
        public void Beat(float a, float b)
        {
            Beat_T = b;
        }

        public float Beat_T { get; private set; } = 0f;

        public Action<float, float> OnBeat => Beat;
    }
}