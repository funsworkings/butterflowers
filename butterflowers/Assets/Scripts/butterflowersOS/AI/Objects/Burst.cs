using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace butterflowersOS.AI.Objects
{
	[System.Serializable]
	public class Burst
	{
		public Vector3 position = Vector3.zero;
		public Vector3 velocity;
		
		public List<Vector3> nodes = new List<Vector3>();
		float distanceThreshold = 0f;

		public Burst(Vector3 velocity, float distanceThreshold)
		{
			nodes.Add(position);
			
			this.velocity = velocity;
			this.distanceThreshold = distanceThreshold;
		}

		public void Continue(float dt, float g)
		{
			velocity += (Vector3.down * g * dt);
			position += velocity * dt;

			if (Vector3.Distance(position, nodes.Last()) > distanceThreshold) 
			{
				nodes.Add(position); // Push node position
			}
		}
	}
}