using System;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.AI.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace butterflowersOS.AI
{
	public class Flower : Shape
	{
		int nodes = 0;
		int petals = 0;
		float radius = 0f;

		[SerializeField] int minPetals = 3, maxPetals = 6;
		[SerializeField] float minRadius = 1f, maxRadius;
		[SerializeField] int minNodes = 3, maxNodes = 16;

		List<Vector3> nodeOffsets = new List<Vector3>();

		protected override void OnEnable()
		{
			base.OnEnable();

			petals = Random.Range(minPetals, maxPetals);
			radius = (Math.Abs(minRadius - maxRadius) < .01f)? minRadius: Random.Range(minRadius, maxRadius);
			nodes = (minNodes == maxNodes)? minNodes:Random.Range(minNodes, maxNodes);
			nodeOffsets = new List<Vector3>();

			for (int i = 0; i < nodes; i++) 
			{
				Vector3 node = Vector3.zero;
				float angle = 2f * Mathf.PI * (1f * i / nodes);

				float len = radius * Mathf.Cos(petals * angle);
				node = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

				node *= (len / node.magnitude);
				
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
			GL.Begin(GL.LINE_STRIP);
			
			Vector3 firstNode = Vector3.zero;
			for (int i = 0; i < nodes; ++i)
			{
				// Vertex colors change from red to green
				GL.Color(color);
				// Another vertex at edge of circle

				if (i == 0) firstNode = nodeOffsets[i];

				var node = nodeOffsets[i];
				GL.Vertex3(node.x, node.y, node.z);
			}
			GL.Vertex3(firstNode.x, firstNode.y, firstNode.z);

			GL.End();
			GL.PopMatrix();
		}

		public static DrawCall Draw(float radius, int petals, int nodes)
		{
			List<Vector3> nodeOffsets = new List<Vector3>();
			for (int i = 0; i < nodes; i++) 
			{
				Vector3 node = Vector3.zero;
				float angle = 2f * Mathf.PI * (1f * i / nodes);

				float len = radius * Mathf.Cos(petals * angle);
				node = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

				node *= (len / node.magnitude);
				
				nodeOffsets.Add(node);
			}
			nodeOffsets.Add(nodeOffsets.First());

			DrawCall drawOp = new DrawCall 
			{
				GL_TYPE = GL.LINE_STRIP,
				nodes = nodeOffsets.ToArray()
			};
			
			return drawOp;
		}
	}
}