using butterflowersOS.AI.Objects;
using UnityEngine;
using uwu.Extensions;

namespace butterflowersOS.AI
{
	public class Circle : Shape
	{
		// When added to an object, draws colored rays from the
		// transform position.
		public int lineCount = 100;
		float radius = 0f;

		[SerializeField] float growRate = 1f;
		float m_growRate = 0f;

		protected override void OnEnable()
		{
			base.OnEnable();
			radius = 0f;
		}

		public override void Trigger(float saturation, float value, Color baseline, params object[] data)
		{
			base.Trigger(saturation, value, baseline, data);

			m_growRate = Mathf.Pow(saturation, 2f).RemapNRB(0f, 1f, .3f, 1f) * growRate;
		}

		protected override void Update()
		{
			base.Update();

			radius += Time.deltaTime * m_growRate;
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