using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class VirtualCursor : Cursor
{
    RectTransform rect;

    [SerializeField] float sp = 1f;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    protected override void Update()
    {
        base.Update();

        Vector2 offset = Vector2.zero;

        if (Input.GetKey(KeyCode.H)) offset.x += sp;
        if (Input.GetKey(KeyCode.F)) offset.x -= sp;
        if (Input.GetKey(KeyCode.T)) offset.y += sp;
        if (Input.GetKey(KeyCode.G)) offset.y -= sp;

        offset *= Time.deltaTime;

        rect.position += new Vector3(offset.x, offset.y, 0f);
    }

    protected override Vector3 Position()
    {
        return rect.position;
    }
}
