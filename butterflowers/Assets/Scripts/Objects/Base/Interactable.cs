using UnityEngine;
using uwu.Gameplay;

namespace Objects.Base
{
    public class Interactable : Entity, IInteractable
    {
        
        protected virtual void Awake(){}
        
        #region IInteractable impl
        
        public void Hover(RaycastHit hit)
        {
            onHover(hit.point, hit.normal);
        }

        public void Unhover()
        {
            onUnhover();
        }

        public void Grab(RaycastHit hit)
        {
            onGrab(hit.point, hit.normal);
        }

        public void Continue(RaycastHit hit)
        {
            onContinue(hit.point, hit.normal);
        }

        public void Release(RaycastHit hit)
        {
            onRelease(hit.point, hit.normal);
        }
        
        #endregion

        #region Interactable callbacks

        protected virtual void onHover(Vector3 point, Vector3 normal)
        {

        }

        protected virtual void onUnhover()
        {

        }

        protected virtual void onGrab(Vector3 point, Vector3 normal)
        {

        }

        protected virtual void onContinue(Vector3 point, Vector3 normal)
        {

        }

        protected virtual void onRelease(Vector3 point, Vector3 normal)
        {

        }

        #endregion
    }
}
