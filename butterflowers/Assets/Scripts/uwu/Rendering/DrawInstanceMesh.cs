using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DrawInstanceMesh : MonoBehaviour
{
    Mesh mesh;
    Material mat;

    [SerializeField] Transform root = null;

    Matrix4x4[] matrix;
    ShadowCastingMode castShadows;

    bool turnOnInstance = true;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        mat = GetComponent<Renderer>().material;
        matrix = new Matrix4x4[2] { root.localToWorldMatrix, this.transform.localToWorldMatrix };
        castShadows = ShadowCastingMode.On;

        Graphics.DrawMeshInstanced(mesh, 0, mat, matrix, matrix.Length, null, castShadows, true, 0, null);
    }

    void Update()
    {
        if (turnOnInstance) {
            //mesh = GetComponent<MeshFilter>().mesh;
            //mat = GetComponent<Renderer>().material;
            //castShadows = ShadowCastingMode.On;

            matrix = new Matrix4x4[2] { root.transform.localToWorldMatrix, this.transform.localToWorldMatrix };
            Graphics.DrawMeshInstanced(mesh, 0, mat, matrix, matrix.Length, null, castShadows, true, 0, null);
        }
    }
}
