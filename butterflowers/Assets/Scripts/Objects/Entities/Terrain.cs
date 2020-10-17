using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{

	#region Properties

	uwu.Gameplay.Interactable interactable;

	#endregion

	#region Attributes

	[SerializeField] GameObject vinePrefab;
    [SerializeField] Transform vineRoot;

	#endregion

	#region Monobehaviour callbacks

	void Awake()
    {
        interactable = GetComponent<uwu.Gameplay.Interactable>();
    }

    void OnEnable()
    {
        interactable.onGrab += DropVine;
    }

    void OnDisable()
    {
        interactable.onGrab -= DropVine;
    }

	#endregion

	#region Vine operations

	void DropVine(Vector3 position, Vector3 normal) 
    {
        var instance = Instantiate(vinePrefab, vineRoot);
            instance.transform.position = position;
            instance.transform.up = transform.forward;
    }

	#endregion
}
