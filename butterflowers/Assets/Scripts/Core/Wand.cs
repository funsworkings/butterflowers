using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wand : MonoBehaviour
{
    new Camera camera;
    
    [SerializeField] Cursor cursor;
    [SerializeField] float distanceFromCamera = 10f;

    [Header("Interaction")]
        Ray ray;
        RaycastHit raycastHit;
       
        [SerializeField] 
        LayerMask interactionMask;
        [SerializeField]
        float interactionDistance = 100f;

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
            return Input.GetMouseButton(0);
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

        if (down || cont || up)
            Interact();
    }

    #region Interaction

    void Interact()
    {
        ray = camera.ScreenPointToRay(position);

        var hits = Physics.RaycastAll(ray, interactionDistance, interactionMask.value);
        if (hits.Length > 0)
            ParseInteractions(hits);
    }

    void ParseInteractions(RaycastHit[] hits)
    {
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
            }
            Debug.LogFormat("Collided with {0}", hit.collider.gameObject.name);
        }
    }

    #endregion
}