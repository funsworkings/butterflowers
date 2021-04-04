using System;
using System.Collections.Generic;
using butterflowersOS.AI.Objects;
using UnityEngine;

namespace butterflowersOS.AI
{
	public class Wrapper : MonoBehaviour
	{
		// Properties

		BoxCollider collider;
		Driver driver;

		public Vector3 t0, t1, t2, t3;
		public Vector3 b0, b1, b2, b3;
		
		// Attributes
		
		[SerializeField] int refreshRate = 10;
		[SerializeField] int refresh = 0;
		
		[SerializeField] int nodeDensity = 10;
		[SerializeField] GameObject nodePrefab = null;
		
		// Collection
		
		List<Node> nodes = new List<Node>();

		#region Accessors

		Bounds bounds => collider.bounds;
		Vector3 boundaries => collider.bounds.extents;

		public List<Node> Nodes => nodes;
		
		#endregion

		void Awake()
		{
			collider = GetComponent<BoxCollider>();
			driver = GetComponent<Driver>();
		}

		void Update()
		{
			if (refresh++ > refreshRate) 
			{
				refresh = 0;

				var entities = driver.ActiveEntities;
				foreach(Entity e in entities) { if(!e.WillWrapDuringFrame) WrapEntity(e);}
			}
		}

		void OnDrawGizmos()
		{
			if (collider == null) return;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(transform.position, boundaries * 2f);
		}

		public void Setup(int density)
		{
			GetCorners();

			nodeDensity = density;
			CreateNodes();
		}

		void GetCorners()
		{
			Vector3 origin = transform.position;
			Vector3 extents = boundaries;

			t0 = origin + new Vector3(extents.x, extents.y, extents.z);
			t1 = origin + new Vector3(extents.x, extents.y, -extents.z);
			t2 = origin + new Vector3(-extents.x, extents.y, -extents.z);
			t3 = origin + new Vector3(-extents.x, extents.y, extents.z);
			
			b0 = origin + new Vector3(extents.x, -extents.y, extents.z);
			b1 = origin + new Vector3(extents.x, -extents.y, -extents.z);
			b2 = origin + new Vector3(-extents.x, -extents.y, -extents.z);
			b3 = origin + new Vector3(-extents.x, -extents.y, extents.z);
		}

		void CreateNodes()
		{
			Vector3 origin = b2;
			Vector3 extents = boundaries * 2f;
			
			float w = extents.x;
			float h = extents.y;
			float d = extents.z;

			float _i = (1f / nodeDensity);

			for (int i = 0; i < nodeDensity; i++) {
				for (int j = 0; j < nodeDensity; j++) {
					for (int k = 0; k < nodeDensity; k++) {

						Vector3 point = origin + new Vector3(extents.x * (i * _i), extents.y * (j * _i), extents.z * (k * _i));

						Node node = new Node {point = point};
						var g = Instantiate(nodePrefab, transform);
						g.transform.position = point;
						g.transform.localScale = Vector3.one * .33f;
						nodes.Add(node);
					}
				}
			}
		}

		#region Ops
		
		void WrapEntity(Entity e)
		{
			Vector3 origin = transform.position;
			Vector3 position = e.transform.position;
			
			if (!bounds.Contains(position)) // Outside of bounds
			{
				Ray ray = new Ray(origin, (position - origin));
				float distance = 0f;
				bool intersects = bounds.IntersectRay(ray, out distance);

				if (intersects) {
					if (distance < 0f) distance *= -1f;

					Vector3 vector = ray.direction;
					Vector3 dir = vector.normalized;
					
					float offset = Mathf.Repeat(vector.magnitude, distance);
					position = origin - (vector.normalized * (distance - offset));

					Vector3 from = origin - (dir * distance);
					Vector3 to = from + (dir * offset);

					e.PreWrap(from, to);
				}
			}
		}
		
		#endregion
	}
}