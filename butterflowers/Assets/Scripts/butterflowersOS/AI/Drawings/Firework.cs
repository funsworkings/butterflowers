using System;
using System.Collections.Generic;
using butterflowersOS.AI.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace butterflowersOS.AI
{
	public class Firework : Shape
	{
		int nodes = 0;

		[SerializeField] int minNodes = 3, maxNodes = 16;
		[SerializeField] float minSpeed = 1f, maxSpeed = 10f;
		[SerializeField] float minArc = 0f, maxArc = 90f;
		[SerializeField] float burstUpdateThreshold = 1f;
		
		List<Burst> bursts = new List<Burst>();

		protected override void OnEnable()
		{
			base.OnEnable();

			nodes = (minNodes == maxNodes)? minNodes:Random.Range(minNodes, maxNodes);
			bursts = new List<Burst>();

			for (int i = 0; i < nodes; i++) 
			{
				float arc = Random.Range(minArc, maxArc);
				float speed = Random.Range(minSpeed, maxSpeed);
				float angle = Random.Range(0f, 360f);
				
				Vector3 vel = Quaternion.Euler(-arc, angle, 0f) * Vector3.forward;
				vel *= speed;

				bursts.Add(new Burst(vel, burstUpdateThreshold));
			}
		}

		protected override void Update()
		{
			base.Update();
			
			UpdateBursts();
		}

		protected override void DidRenderShape()
		{
			GL.PushMatrix();
			// Set transformation matrix for drawing to
			// match our transform
			GL.MultMatrix(transform.localToWorldMatrix);

			for (int i = 0; i < nodes; ++i)
			{
				// Vertex colors change from red to green
				GL.Color(color);
				// Another vertex at edge of circle

				var node = bursts[i];
				var _nodes = node.nodes;
				
				// Draw lines
				GL.Begin(GL.LINE_STRIP);
				foreach(Vector3 _node in _nodes) GL.Vertex3(_node.x, _node.y, _node.z);
				GL.End();
			}

			
			GL.PopMatrix();
		}

		void UpdateBursts()
		{
			foreach (Burst burst in bursts) 
			{
				burst.Continue(Time.deltaTime, Physics.gravity.magnitude / 2f);
			}
		}
	}
}