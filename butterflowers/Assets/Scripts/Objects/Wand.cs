using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wand : MonoBehaviour
{
    new Camera camera;

    [SerializeField] bool m_spells = true;

    [SerializeField] Cursor cursor;
    [SerializeField] float distanceFromCamera = 10f;

    [Header("Interaction")]
        Ray ray;
        RaycastHit raycastHit;
       
        [SerializeField] 
        LayerMask interactionMask, navigationMask;
        [SerializeField]
        float interactionDistance = 100f;

        [SerializeField] List<Interactable> interacting = new List<Interactable>();

        [SerializeField] bool down, cont, up;   


    public Vector3 position {
        get{
            return cursor.position;
        }
        set {
            transform.position = value;
        }
    }

    public Vector3 worldPosition {
        get
        {
            if (camera == null)
                return Vector3.zero;

            return camera.ScreenToWorldPoint(new Vector3(position.x, position.y, 10f));
        }
    }

    public bool spells
    {
        get {
            return m_spells;
        }
    }

    public Vector3 velocity {
        get{
            return cursor.velocity;
        }
    }

    public float speed {
        get{
            return cursor.speed;
        }
    }

    #region Monobehaviour callbacks

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        if (camera == null)
            return;

        down = Input.GetMouseButtonDown(0);
        cont = Input.GetMouseButton(0);
        up = Input.GetMouseButtonUp(0);

        Interact();

        if (cont) Waypoint();
    }

    #endregion

    #region Navigation

    void Waypoint()
    {
        ray = camera.ScreenPointToRay(position);

        var hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, interactionDistance, navigationMask.value)){
            //wizard.MoveTo(hit.point); // Move wizard target to location
        }
    }

	#endregion

	#region Interaction

	void Interact()
    {
        ray = camera.ScreenPointToRay(position);

        var hits = Physics.RaycastAll(ray, interactionDistance, interactionMask.value);
        ParseInteractions(hits);
    }

    void ParseInteractions(RaycastHit[] hits)
    {
        List<Interactable> interacting = new List<Interactable>();

        if(hits != null){
            GameObject obj = null;
            Interactable obj_int = null;

            foreach(RaycastHit hit in hits)
            {
                obj = hit.collider.gameObject;
                obj_int = obj.GetComponent<Interactable>();

                if(obj_int != null)
                {
                    if (down)
                        obj_int.Grab(hit);
                    else if (cont)
                        obj_int.Continue(hit);
                    else if (up)
                        obj_int.Release(hit);
                    else {
                        obj_int.Hover(hit);
                        interacting.Add(obj_int);
                    }
                }

                //Debug.LogFormat("Collided with {0}", hit.collider.gameObject.name);
            }
        }

        ParseLastInteractions(interacting);
    }

    void ParseLastInteractions(List<Interactable> current){
        for(int i = 0; i < interacting.Count; i++){
            var interactable = interacting[i];
            if(!current.Contains(interactable))
                interactable.Unhover();
        }

        interacting = current;
    }

    #endregion
}