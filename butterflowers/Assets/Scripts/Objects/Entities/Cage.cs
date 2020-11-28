using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode.Examples.MathNodes;

public class Cage : MonoBehaviour
{
    
    // Events

    public System.Action onUpdateActiveCorners;
    public System.Action onCompleteCorners;
    
    #region Internal

    [Serializable]
    public struct Vertex
    {
        public Vector3 top;
        public Vector3 bottom;
        
        public bool active;
    }
    
    #endregion
    
    // Properties

    new Collider collider;
    
    // Attributes

    Bounds bounds;
    
    bool[] m_active = new bool[4];
    bool[] m_queue = new bool[4];
    
    // Collections
    
    [SerializeField] Vertex[] vertices = new Vertex[4];
    
    #region Accessors

    public bool[] active => m_active;
    public bool[] queue => m_queue;

    #endregion
    
    #region Monobehaviour callbacks

    void Awake()
    {
        collider = GetComponent<Collider>();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        
        
        foreach (Vertex v in vertices) {
            Handles.color = Color.blue;
            Handles.DrawWireCube(v.top, Vector3.one);

            Handles.color = Color.red;
            Handles.DrawWireCube(v.bottom, Vector3.one);
        }
        
    }
#endif

    #endregion
    
    #region Initialization

    public void Initialize(bool[] active)
    {
        m_active = active;
        
        CalculateVerticesAndBounds();
        CheckIfComplete();
    }
    
    #endregion
    
    #region Bounds and vertices

    void CalculateVerticesAndBounds()
    {
        bounds = collider.bounds;

        float height = bounds.size.y;
        float bottomOffset = -height / 2f;
        
        float width = bounds.extents.x;
        float depth = bounds.extents.z;

        Vector3 upwardsVector = transform.TransformVector(0f, 1f, 0f);
        
        List<Vertex> tempVertices = new List<Vertex>();
        Vertex vertex = new Vertex();

        // Front-left
            vertex.bottom = transform.TransformPoint(new Vector3(-width, bottomOffset, depth));
            vertex.top = vertex.bottom + upwardsVector * (-bottomOffset*2f);
            vertex.active = active[0];
            tempVertices.Add(vertex);
        
        // Front-right
            vertex.bottom = transform.TransformPoint(new Vector3(width, bottomOffset, depth));
            vertex.top = vertex.bottom + upwardsVector * (-bottomOffset*2f);
            vertex.active = active[1];
            tempVertices.Add(vertex);
        
        // Back-left
            vertex.bottom = transform.TransformPoint(new Vector3(-width, bottomOffset, -depth));
            vertex.top = vertex.bottom + upwardsVector * (-bottomOffset*2f);
            vertex.active = active[2];
            tempVertices.Add(vertex);
        
        // Back-right
            vertex.bottom = transform.TransformPoint(new Vector3(width, bottomOffset, -depth));
            vertex.top = vertex.bottom + upwardsVector * (-bottomOffset*2f);
            vertex.active = active[3];
            tempVertices.Add(vertex);

        vertices = tempVertices.ToArray();
    }

    public Vertex GetClosestVertex(Vector3 point, out int index)
    {
        float bestDistance = Vector3.Distance(point, vertices[0].bottom);
        Vertex bestVertex = vertices[0];
        
        index = 0;

        for (int i = 1; i < vertices.Length; i++) {
            float dist = Vector3.Distance(point, vertices[i].bottom);
            if (dist <= bestDistance) 
            {
                bestDistance = dist;
                bestVertex = vertices[i];
                index = i;
            }
        }

        return bestVertex;
    }

    #endregion
    
    #region Activation

    public bool ActivateVertex(int index)
    {
        index = Mathf.Clamp(index, 0, vertices.Length-1);
        if (vertices[index].active)
            return false;

        vertices[index].active = true;
        m_active[index] = true;

        if (onUpdateActiveCorners != null)
            onUpdateActiveCorners();

        CheckIfComplete();
        
        return true;
    }

    public bool HoldVertex(int index)
    {
        if (queue[index])
            return false;

        queue[index] = true;
        return true;
    }

    void CheckIfComplete()
    {
        bool complete = true;
        foreach (Vertex v in vertices) {
            if (!v.active) {
                complete = false;
                break;
            }
        }

        if (complete) {

            if (onCompleteCorners != null)
                onCompleteCorners();

        }
    }
    
    #endregion
}
