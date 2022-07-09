using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace live_simulation
{
    public class BridgeUtil : MonoBehaviour
    {
        // Events

        public static System.Action onCameraChange;
        public static System.Action onCycleDay;
        public static System.Action<string> onCreateImage;
        
        // Properties

        [SerializeField] private string additiveSceneName;
        
        private void Start()
        {
            SceneManager.LoadScene(additiveSceneName, LoadSceneMode.Additive);
        }

        private void OnDestroy()
        {
            SceneManager.UnloadScene(additiveSceneName);
        }
    }
}