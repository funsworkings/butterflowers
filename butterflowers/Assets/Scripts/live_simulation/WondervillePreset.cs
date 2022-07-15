using butterflowersOS;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace live_simulation
{
    [CreateAssetMenu(fileName = "New WP", menuName = "Custom/WP", order = 0)]
    public class WondervillePreset : ScriptableObject
    {
        [Header("AI")]
        [UnityEngine.Min(1)] public float idleTime = 6f;
        [UnityEngine.Min(3)] public int queryWidth = 10;
        [UnityEngine.Min(3)] public int queryHeight = 10;
        [UnityEngine.Range(.3f, 1f)] public float queryViewportWidth = 1f;
        [UnityEngine.Range(.3f, 1f)] public float queryViewportHeight = 1f;
        public float markerScaleMultiplier = 1f;
        [UnityEngine.Min(.1f)] public float markerInstantiateTime = .1f;
        [UnityEngine.Min(.1f)] public float stepBetweenFailureMarkers = 2f;
        [UnityEngine.Min(.1f)] public float stepBetweenValidMarker = 1.5f;
        [UnityEngine.Min(.1f)] public float stepAfterValidMarker = 2f;
        public float beaconSpawnDistanceFromCamera = 1f;
        public float beaconPostSpawnDelay = 1.5f;
        public EVENTCODE overrideEventCode = EVENTCODE.NULL;
        public int beatsToWaitWebcamIdle = 2;
        
        [Header("Webcam")] 
        [UnityEngine.Min(.3f)] public float selectionBoxTransitionTime = 1f;
        [UnityEngine.Min(.3f)] public float selectionBoxWaitTime = 1f;
        
        [Header("Heatmap")]
        [UnityEngine.Min(1)] public float heatmapUpdateInterval = 5f;
        [UnityEngine.Min(1)] public float heatmapSmoothingTime = 10f;
        
        [Header("Legacy")]
        [UnityEngine.Min(1)] public float textureOverrideTimeLength = 3f;

        [Header("Debug")] 
        public bool showDebugMenu = false;
    }
}