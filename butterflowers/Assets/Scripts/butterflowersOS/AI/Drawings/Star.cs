using System;
using System.Collections.Generic;
using butterflowersOS.AI.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace butterflowersOS.AI
{
	public class Star : Shape
	{
		int nodes = 0;
		float radius = 0f;
		
		[SerializeField] float minRadius = 1f, maxRadius;
		[SerializeField] int minNodes = 3, maxNodes = 16;
		[SerializeField] bool normalizeLines = false;
		
		List<Vector3> nodeOffsets = new List<Vector3>();

		protected override void OnEnable()
		{
			base.OnEnable();

			radius = (Math.Abs(minRadius - maxRadius) < .01f)? minRadius: Random.Range(minRadius, maxRadius);
			nodes = (minNodes == maxNodes)? minNodes:Random.Range(minNodes, maxNodes);
			nodeOffsets = new List<Vector3>();

			for (int i = 0; i < nodes; i++) 
			{
				Vector3 node = Random.insideUnitSphere * radius;
				if (normalizeLines) {
					var offset = Vector3.one;
					offset.Scale(node / 1f);
					
					node.Scale(offset);
				}
				
				nodeOffsets.Add(node);
			}
		}

		protected override void DidRenderShape()
		{
			GL.PushMatrix();
			// Set transformation matrix for drawing to
			// match our transform
			GL.MultMatrix(transform.localToWorldMatrix);

			// Draw lines
			GL.Begin(GL.LINES);
			
			Vector3 firstNode = Vector3.zero;
			for (int i = 0; i < nodes; ++i)
			{
				// Vertex colors change from red to green
				GL.Color(color);
				// Another vertex at edge of circle

				var node = nodeOffsets[i];
				
				GL.Vertex3(0, 0, 0); // Return to position
				GL.Vertex3(node.x, node.y, node.z);
			}

			GL.End();
			GL.PopMatrix();
		}
	}
}