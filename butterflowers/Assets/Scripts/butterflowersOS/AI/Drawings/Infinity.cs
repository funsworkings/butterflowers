using butterflowersOS.AI.Objects;
using UnityEngine;

namespace butterflowersOS.AI
{
	public class Infinity : Shape
	{
		protected override void DidRenderShape()
		{
			GL.PushMatrix();
			// Set transformation matrix for drawing to
			// match our transform
			GL.MultMatrix(transform.localToWorldMatrix);

			// Draw lines
			GL.Begin(GL.LINE_STRIP);
			
			Vector3 firstNode = Vector3.zero;
			for (int i = 0; i < lineCount; ++i)
			{
				float a = i / (float)lineCount;
				float angle = a * Mathf.PI * 2;
				
				// Vertex colors change from red to green
				GL.Color(color);
				// Another vertex at edge of circle

				var node = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
				if (i == 0)
					firstNode = node;
				
				GL.Vertex3(node.x, node.y, 0);
			}
			GL.Vertex3(firstNode.x, firstNode.y, 0);
			
			GL.End();
			GL.PopMatrix();
		}
	}
}