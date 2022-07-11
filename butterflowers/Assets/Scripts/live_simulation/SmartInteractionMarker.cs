using System;
using System.Collections.Generic;
using butterflowersOS;
using butterflowersOS.Objects.Base;
using UnityEngine;
using UnityEngine.UI;

namespace live_simulation
{
    public class SmartInteractionMarker : MonoBehaviour
    {
        // Properties

        public RaycastHit HitInfo { get; set; } = default(RaycastHit);
        public Entity HitEntity { get; set; } = null;
        public List<EVENTCODE> HitEvents { get; set; } = null;
        
        [SerializeField] private Image _image;
        [SerializeField] private Sprite on, off;

        private void OnEnable()
        {
            _image.enabled = false;
        }

        // x = 0-1
        // y = 0-1
        public void Setup(bool hit, RaycastHit hitInfo, Entity hitEntity, List<EVENTCODE> hitEvents)
        {
            _image.sprite = (hit) ? on : off;
            _image.enabled = true;

            HitInfo = hitInfo;
            HitEntity = hitEntity;
            HitEvents = hitEvents;
        }
    }
}