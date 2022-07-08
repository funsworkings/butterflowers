using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace live_simulation
{
    public class PrimaryController : MonoBehaviour
    {
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