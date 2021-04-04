using System.Collections.Generic;
using butterflowersOS.AI.Objects;
using UnityEngine;

namespace butterflowersOS.AI
{
	public class Trail : Shape
	{
		[SerializeField] int nodeStackHeight = 10;
		
		List<Vector3> nodes = new List<Vector3>();

		protected override void OnEnable()
		{
			base.OnEnable();

			nodes = new List<Vector3>(); // Clear node stack
		}

		protected override void DidRenderShape()
		{
			GL.PushMatrix();
			// Set transformation matrix for drawing to
			// match our transform
			//GL.MultMatrix(transform.localToWorldMatrix);

			// Draw lines
			GL.Begin(GL.LINE_STRIP);

			for (int i = 0; i < nodes.Count; ++i)
			{
				// Vertex colors change from red to green
				GL.Color(color);
				// Another vertex at edge of circle

				var node = nodes[i];
				GL.Vertex3(node.x, node.y, node.z);
			}
			
			GL.End();
			GL.PopMatrix();
		}

		public void Push(Vector3 point)
		{
			if(nodes.Count >= nodeStackHeight) nodes.RemoveAt(0);
			nodes.Add(point);
		}

		public void Clear()
		{
			nodes = new List<Vector3>();
		}
	}
}