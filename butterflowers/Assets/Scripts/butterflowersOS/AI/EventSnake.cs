using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace butterflowersOS.AI
{
	public class EventSnake : MonoBehaviour
	{
		// Properties

		LineRenderer line;
		
		[SerializeField] BoxCollider bounds;
		[SerializeField] List<EVENTCODE> events = new List<EVENTCODE>();
		
		[SerializeField] float refresh = 0f, refresh_t = 0f;
		[SerializeField] int eventIndex = 0;
		
		// Attributes

		[SerializeField] int maxLength = 10;
		[SerializeField, Range(.01f, 1f)] float fill = 1f;
		
		// Collections
		
		List<Vector3> _nodes = new List<Vector3>();
		

		void Awake()
		{
			line = GetComponent<LineRenderer>();
		}

		void Update()
		{
			if (refresh > 0f && eventIndex < events.Count) 
			{
				refresh_t += Time.deltaTime;
				if (refresh_t >= refresh) 
				{
					var @event = events[eventIndex];
					DrawNode(eventIndex, @event);
					
					refresh_t = 0f;
					++eventIndex;
				}
			}
		}

		#region Ops

		public void Push(sbyte[] _events, float duration)
		{
			events  = new List<EVENTCODE>();
			
			var size = _events.Length;
			if (size > 0) 
			{
				refresh = (duration / (size+1));
				foreach(sbyte @event in _events) events.Add((EVENTCODE)@event);
			}
			else 
			{
				refresh = 0f;
			}

			refresh_t = 0f;
			eventIndex = 0;
		}
		
		#endregion
		
		#region Drawing

		void DrawNode(int index, EVENTCODE @event)
		{
			Vector3 point = GetNodePosition(index);
			_nodes.Add(point);

			int length = Mathf.Min(_nodes.Count, maxLength);
			int subset = _nodes.Count - length;

			line.positionCount = length;
			line.SetPositions(_nodes.GetRange(subset, length).ToArray());
		}

		Vector3 GetNodePosition(int index)
		{
			var volume = bounds.bounds;
			var extents = volume.extents;
			
			Vector3 offset = new Vector3(Random.Range(-extents.x, extents.x), Random.Range(-extents.y, extents.y), Random.Range(-extents.z, extents.z)) * fill;
			return volume.center + offset;
		}
		
		#endregion
	}
}