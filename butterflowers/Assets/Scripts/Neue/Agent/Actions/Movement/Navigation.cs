using Neue.Agent.Presets;
using UnityEngine;
using UnityEngine.AI;

namespace Neue.Agent.Actions.Movement
{
	public class Navigation : MonoBehaviour
	{
		// Events

		public System.Action<Vector3> onArriveAtDestination;
		
		// Properties

		NavMeshAgent _navMeshAgent;
		NavMeshPath _path;
		int _pathIndex = 1;
		
		Vector3 _position;
		Vector3 _next = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		Vector3 _destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		
		[SerializeField] MovementPreset preset;
		[SerializeField] float speed = 0f;
		
		[SerializeField] bool moving = false;

		// Attributes

		[Header("Debug")]	
		public Camera debugCamera;
		public LayerMask debugNavigationMask;

		
		#region Accessors

		float stoppingDistance => _navMeshAgent.stoppingDistance;

		float moveSpeed (float angleBetween){ return preset.GetMoveSpeedFromBearing(angleBetween); }
		float turnSpeed (float angleBetween){ return preset.GetTurnSpeedFromBearing(angleBetween); }
		
		#endregion
		
		
		void Awake()
		{
			_navMeshAgent = GetComponent<NavMeshAgent>();
		}

		void Start()
		{
			debugCamera = Camera.main;
			
			_navMeshAgent.isStopped = true;
			_path = new NavMeshPath();
			
			_next = _navMeshAgent.nextPosition;
		}
		
		void Update()
	    {
			SetFinalAgentPosition(); // Apply position to agent
			
			if (Input.GetKey(KeyCode.Alpha1)) 
			   WaitForTarget();

			if (_path.corners == null || _path.corners.Length == 0) return; // Check valid path

			bool completedPath = !SetWaypoint();
			
			Vector3 direction = (_next - _position).normalized;
			float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

			if (float.IsNaN(angle)) angle = 0f;
			float abs_angle = Mathf.Abs(angle);

			RotateTowardsDestination(direction, abs_angle);
			//if (completedPath) return;
			
			bool isNextValid = (_next != _destination);
			if (isNextValid) MoveTowardsDestination(abs_angle);
			else speed = 0f;
	    }
		
		void OnDrawGizmos()
		{
			DrawTarget();
		}
		
		#region Pathing

		void ConstructNewPath()
		{
			_path = new NavMeshPath();

			_navMeshAgent.CalculatePath(_destination, _path);
			_pathIndex = 1;
			_navMeshAgent.isStopped = false;
		}

		bool SetWaypoint()
		{
			if (_pathIndex >= _path.corners.Length) 
			{
				_next = _destination;
				_navMeshAgent.isStopped = true;
				return false;
			}
			else
			{
				_next = _path.corners[_pathIndex];
				return true;
			}
		}
		
		#endregion
		
		#region Position

		void MoveTowardsDestination(float bearing)
		{
			float distance = Vector3.Distance(_position, _next);
			if (distance > stoppingDistance) 
			{
				float t_speed = moveSpeed(bearing);
				speed = Mathf.Lerp(speed, t_speed, preset.acceleration * Time.deltaTime);
				
				Vector3 movement = transform.forward * Time.deltaTime * speed;
				_navMeshAgent.Move(movement);
			}
			else
			{
				++_pathIndex;
				if (_pathIndex >= _path.corners.Length) 
				{
					_next = _destination;
					_navMeshAgent.isStopped = true;
	                    
					ArriveAtDestination();
				}
			}
		}
		
		void SetFinalAgentPosition()
		{
			NavMeshHit hit;
			if(NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
			{
				_position = hit.position;
			}
		}
		
		#endregion
		
		#region Rotation

		void RotateTowardsDestination(Vector3 direction, float bearing)
		{
			var newDir = Vector3.RotateTowards(transform.forward, direction, 50 * Time.deltaTime, 0.0f);
			var newRot = Quaternion.LookRotation(newDir);
	            
			transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * turnSpeed(bearing));
		}
		
		#endregion

		#region Destination

		void SetDestination(Vector3 point)
		{
			_destination = point;
			ConstructNewPath();

			moving = WithinStoppingDistance(_destination);
		}
		
		void ArriveAtDestination()
		{
			if (onArriveAtDestination != null)
				onArriveAtDestination(_destination);
		}

		bool WithinStoppingDistance(Vector3 point)
		{
			float distance = Vector3.Distance(transform.position, point);
			return distance <= stoppingDistance;
		}
		
		#endregion
		
		#region Debug

		void WaitForTarget()
		{
			var ray = debugCamera.ScreenPointToRay(Input.mousePosition);
			var hit = new RaycastHit();

			if (Physics.Raycast(ray, out hit, 999f, debugNavigationMask.value)) {
				if (Input.GetMouseButton(0)) 
					SetDestination(hit.point);
			}
		}

		void DrawTarget()
		{
			Gizmos.color = Color.blue;
			if(_path != null && _path.corners != null && _path.corners.Length > 0)
			{
				var prev = _position;
				for(int i = _pathIndex; i < _path.corners.Length; ++i)
				{
					Gizmos.DrawLine(prev, _path.corners[i]);
					prev = _path.corners[i];
				}
			}
		}
		
		#endregion
	}
}