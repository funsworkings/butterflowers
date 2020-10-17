using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uwu.Extensions;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryLine : MonoBehaviour
{
    new LineRenderer renderer;

    GameObject root = null;

    [SerializeField] int vertices = 100;
    [SerializeField] float height = 1f;
    [SerializeField] AnimationCurve arcCurve;

    Vector3 a, b;

    public LineRenderer Line => renderer;

    void Awake()
    {
        renderer = GetComponent<LineRenderer>();
    }

    void Start() 
    {
        root = new GameObject("arc_root");
        
        root.transform.parent = transform;
        root.transform.ResetTransformValues(); // ZZero out transform
    }

    public Vector3 GetPointOnArc(Vector3 from, Vector3 to, float t)
    {
        Vector3 dir = (to - from);

        root.transform.position = from;
        root.transform.forward = dir.normalized;

        float y = arcCurve.Evaluate(t) * height;
        float z = dir.magnitude * t;

        return root.transform.TransformPoint(0f, y, z);
    }

    public void DrawArc(Vector3 from, Vector3 to, float length = 1f)
    {
        a = from;
        b = to;

        length = Mathf.Clamp01(length);

        int total = vertices - 1;
        int sub = Mathf.FloorToInt(total * length);

        renderer.positionCount = sub;
        renderer.SetPositions(GetDrawArcVertices(sub));
    }

    Vector3[] GetDrawArcVertices(int count)
    {
        float interval = 0f;

        List<Vector3> verts = new List<Vector3>();
        for (int i = 0; i < count; i++) 
        {
            interval = (float)i / (vertices - 1);
            verts.Add(GetPointOnArc(a, b, interval));
        }

        return verts.ToArray();
    }
}
