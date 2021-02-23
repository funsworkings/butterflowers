using UnityEngine;
using UnityEngine.Rendering;

namespace uwu.Rendering
{
	public class DrawInstanceMesh : MonoBehaviour
	{
		[SerializeField] Transform root = null;
		ShadowCastingMode castShadows;
		Material mat;

		Matrix4x4[] matrix;
		Mesh mesh;

		readonly bool turnOnInstance = true;

		void Start()
		{
			mesh = GetComponent<MeshFilter>().mesh;
			mat = GetComponent<Renderer>().material;
			matrix = new Matrix4x4[2] {root.localToWorldMatrix, transform.localToWorldMatrix};
			castShadows = ShadowCastingMode.On;

			Graphics.DrawMeshInstanced(mesh, 0, mat, matrix, matrix.Length, null, castShadows, true, 0, null);
		}

		void Update()
		{
			if (turnOnInstance) {
				//mesh = GetComponent<MeshFilter>().mesh;
				//mat = GetComponent<Renderer>().material;
				//castShadows = ShadowCastingMode.On;

				matrix = new Matrix4x4[2] {root.transform.localToWorldMatrix, transform.localToWorldMatrix};
				Graphics.DrawMeshInstanced(mesh, 0, mat, matrix, matrix.Length, null, castShadows, true, 0, null);
			}
		}
	}
}