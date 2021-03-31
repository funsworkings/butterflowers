using System;
using butterflowersOS.AI.Objects;
using UnityEngine;

namespace butterflowersOS.AI
{
	public class Wrapper : MonoBehaviour
	{
		// Properties

		BoxCollider collider;
		Driver driver;
		
		// Attributes
		
		[SerializeField] int refreshRate = 10;
		[SerializeField] int refresh = 0;
		
		#region Accessors

		Bounds bounds => collider.bounds;
		Vector3 boundaries => collider.bounds.extents;
		
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
				foreach(Entity e in entities) WrapEntity(e);
			}
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(transform.position, boundaries * 2f);
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
					
					float offset = Mathf.Repeat(ray.direction.magnitude, distance);
					position = origin - (ray.direction.normalized * (distance - offset));
					
					Debug.LogFormat("Wrap entity => {0} , distance => {1}  offset => {2}", e.name, distance, offset);

					e.transform.position = position;
				}
			}
		}
		
		#endregion
	}
}