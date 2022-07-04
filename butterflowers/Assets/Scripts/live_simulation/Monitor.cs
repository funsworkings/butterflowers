using System.Collections;
using live_simulation.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace live_simulation
{
    public class Monitor : MonoBehaviour
    {
        // Properties
    
        [SerializeField] Webcam _webcam;
        [SerializeField] RawImage _webcamTargetImage, _webcamTargetLiveImage;
        private AspectRatioFitter _webcamTargetLiveImageFitter;
        
        // Attributes

        [SerializeField, Range(0f, 1f)] private float _captureWResolution, _captureHResolution;

        IEnumerator Start()
        {
            while (!_webcam.Ready) yield return null;
            CaptureWebcamImage();

            _webcamTargetLiveImageFitter = _webcamTargetLiveImage.GetComponent<AspectRatioFitter>();
        }

        void Update()
        {
            if(Input.GetKeyUp(KeyCode.Space)) CaptureWebcamImage();
            else if(Input.GetKeyUp(KeyCode.RightArrow)) SwitchWebcamDevice();

            var tex = _webcam.CurrentActiveRenderTarget;
            _webcamTargetLiveImage.texture = tex;
            _webcamTargetLiveImageFitter.aspectRatio = (tex != null)? 1f * tex.width / tex.height:1f;
        }

        void CaptureWebcamImage()
        {
            var w = Mathf.FloorToInt(Screen.width * _captureWResolution);
            var h = Mathf.FloorToInt(Screen.height * _captureHResolution);
            
            _webcam.Capture(w, h, result =>
            {
                _webcamTargetImage.texture = result;
            });
        }

        void SwitchWebcamDevice()
        {
            _webcam.RequestNextDevice(null);
        }
    }
}