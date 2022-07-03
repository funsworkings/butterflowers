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
        [SerializeField] RawImage _webcamTargetImage;
        
        // Attributes

        [SerializeField, Range(0f, 1f)] private float _captureWResolution, _captureHResolution;

        IEnumerator Start()
        {
            while (!_webcam.Ready) yield return null;
            CaptureWebcamImage();
        }

        void Update()
        {
            if(Input.GetKeyUp(KeyCode.Space)) CaptureWebcamImage();
            else if(Input.GetKeyUp(KeyCode.RightArrow)) SwitchWebcamDevice();
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

        void SwitchWebcamDevice(string deviceName = null)
        {
            
        }
    }
}