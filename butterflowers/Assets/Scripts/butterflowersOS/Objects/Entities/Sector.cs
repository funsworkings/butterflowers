using UnityEngine;
using UnityEngine.Events;

namespace butterflowersOS.Objects.Entities
{
	public class Sector : MonoBehaviour
	{
		public UnityEvent onActivated;
		
		#region Internal

		public enum Status
		{
			Wait = -1,
			
			Queue = 0,
			Active = 1
		}
		
		#endregion

		// Properties

		[SerializeField] Cage cage;

		[SerializeField] Transform vertex;
		[SerializeField] Status status = Status.Wait;

		bool load = false;

		#region Accessors

		public Status _Status => status;

		public Vector3 top => vertex.position;
		public Vector3 bottom => cage.transform.TransformPoint(new Vector3(vertex.localPosition.x, -vertex.localPosition.y, vertex.localPosition.z));
		
		#endregion

		#region Initialization

		public void Load(Cage cage, int _status)
		{
			status = (Status)_status;
			load = true;
		}

		public bool Activate()
		{
			if (status == Status.Wait) 
			{
				status = Status.Queue;
				onActivated.Invoke();

				return true;
			}

			return false;
		}

		public bool Complete()
		{
			if (status == Status.Queue) 
			{
				status = Status.Active;
				return true;
			}
			
			return false;
		}

		#endregion
	}
}