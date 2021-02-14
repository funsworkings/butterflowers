using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace butterflowersOS.Visuals
{
	public class ThumbnailField : MonoBehaviour
	{
		// External

		World World;
		Library Library;
		
		// Collections

		List<Mesh> _meshes = new List<Mesh>();
		List<MaterialPropertyBlock> _propertyBlocks = new List<MaterialPropertyBlock>();
		
		// Properties

		[SerializeField] Material panelMaterial = null;
						 MaterialPropertyBlock panelPropertyBlock = new MaterialPropertyBlock();
		
		[SerializeField] int panelLayer = 0;
		
		bool load = false;
		
		// Attributes

		[SerializeField] Vector3 panelScale = Vector3.zero;

		IEnumerator Start()
		{
			Library = Library.Instance;
			while (!Library.Completed) yield return null;
			
			panelPropertyBlock = new MaterialPropertyBlock();
			
			load = true;
		}

		void Update()
		{
			if (!load) return;

			var thumbnails = Library.Thumbnails;

			var i = 0;
			foreach (Texture2D tex in thumbnails) 
			{
				DrawThumnbnail(i++, tex, transform.position, transform.rotation);	
			}
		}

		#region Drawing

		void DrawThumnbnail(int index, Texture2D texture, Vector3 position, Quaternion rotation)
		{
			Mesh mesh = RequestMesh(index);
			panelPropertyBlock.SetTexture("_MainTex", texture); // Set main texture in property block
			
			Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, panelScale);
			Graphics.DrawMesh(mesh, matrix, panelMaterial, panelLayer, null, 0, panelPropertyBlock,
				ShadowCastingMode.Off);
		}
		
		#endregion
		
		#region Meshes

		Mesh CreateMesh()
        {
        	Mesh mesh = new Mesh();
        	
        	Vector3[] vertices = new Vector3[4]
        	{
        		new Vector3(0, 0, 0),
        		new Vector3(1, 0, 0),
        		new Vector3(0, 1, 0),
        		new Vector3(1, 1, 0)
        	};
        	
        	mesh.vertices = vertices;

        	return mesh;
        }
		
		Mesh RequestMesh(int index)
		{
			int diff = (_meshes.Count - index - 1);
			if (diff < 0) 
			{
				diff = Mathf.Abs(diff);
				
				for (int i = 0; i < diff; i++) 
				{
					var mesh = CreateMesh();
					_meshes.Add(mesh);
				}
			}

			return _meshes[index];
		}
		
		#endregion
	}
}