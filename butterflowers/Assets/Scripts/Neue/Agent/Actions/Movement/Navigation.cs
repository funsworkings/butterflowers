using Neue.Agent.Presets;
using Neue.Agent.Types;
using UnityEngine;
using UnityEngine.AI;
using uwu.Extensions;

namespace Neue.Agent.Actions.Movement
{
	public class Navigation : Module
	{
		// Events

		public System.Action<Vector3> onArriveAtDestination;
		
		// Properties

		NavMeshAgent _navMeshAgent;
		NavMeshPath _path;
		int _pathIndex = 1;
		
		Vector3 _position;
		float bearing = 0f;
		float turn = 0f;
		float move = 0f;
		
		Vector3 _next = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		Vector3 _destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		
		[SerializeField] MovementPreset preset;
		[SerializeField] float speed = 0f;

		[SerializeField] float movementSpeed = -1f;
		[SerializeField] float rotationSpeed = -1f;
		
		[SerializeField] bool moving = false;

		// Attributes

		[Header("Debug")]	
		public Camera debugCamera;
		public LayerMask debugNavigationMask;

		
		#region Accessors

		float stoppingDistance => _navMeshAgent.stoppingDistance;

		float moveSpeed (float angleBetween){ return preset.GetMoveSpeedFromBearing(angleBetween); }
		float turnSpeed (float angleBetween){ return preset.GetTurnSpeedFromBearing(angleBetween); }

		public float Turn => turn;
		public float Move => Mathf.Clamp01(speed / preset.moveSpeed);
		public float Bearing => bearing;

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

		void SetMaxMoveFromBearing(float bearing)
		{
			movementSpeed = preset.moveSpeed;
		}
		
		void MoveTowardsDestination(float bearing)
		{
			float distance = Vector3.Distance(_position, _next);
			if (distance > stoppingDistance) 
			{
				float t_speed = GetMoveSpeedFromBearing(bearing);
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

		void SetMaxRotationFromBearing(float bearing)
		{
			if (bearing >= preset.quickTurnThreshold) rotationSpeed = preset.quickTurnSpeed;
			else rotationSpeed = preset.maxTurnSpeed;
		}

		void RotateTowardsDestination(Vector3 direction, float bearing)
		{
			var newDir = Vector3.RotateTowards(transform.forward, direction, 50 * Time.deltaTime, 0.0f);
			var newRot = Quaternion.LookRotation(newDir);
	            
			transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * GetTurnSpeedFromBearing(bearing));
		}
		
		#endregion
		
		#region Speeds
		
		float GetTurnSpeedFromBearing(float angle)
		{
			float ang_magnitude = angle.RemapNRB(0f, 180f, 0f, 1f);
			float magnitude = preset.turnSpeedCurve.Evaluate(ang_magnitude);

			turn = magnitude;
			
			float speed = turn * rotationSpeed;
			return speed;
		}

		float GetMoveSpeedFromBearing(float angle)
		{
			float ang_magnitude = angle.RemapNRB(0f, 180f, 0f, 1f);
			float magnitude = preset.moveSpeedCurve.Evaluate(ang_magnitude);

			move = (1f - magnitude);
			return move * movementSpeed;
		}
		
		#endregion

		#region Destination

		void SetDestination(Vector3 point)
		{
			_destination = point;
			ConstructNewPath();

			// Wipe initial speeds
			rotationSpeed = -1f;
			movementSpeed = -1f;

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
		
		#region Module

		public override void Continue()
		{
			SetFinalAgentPosition(); // Apply position to agent
			
			if (Input.GetKey(KeyCode.Alpha1)) 
				WaitForTarget();

			if (_path.corners == null || _path.corners.Length == 0) return; // Check valid path

			bool completedPath = !SetWaypoint();
			
			Vector3 direction = (_next - _position).normalized;
			float angle = bearing = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

			if (float.IsNaN(angle)) angle = 0f;
			float abs_angle = Mathf.Abs(angle);
			
			if(movementSpeed < 0f) SetMaxMoveFromBearing(abs_angle);
			if(rotationSpeed < 0f) SetMaxRotationFromBearing(abs_angle);

			RotateTowardsDestination(direction, abs_angle);
			//if (completedPath) return;
			
			bool isNextValid = (_next != _destination);
			if (isNextValid) MoveTowardsDestination(abs_angle);
			else speed = 0f;
		}

		public override void Pause()
		{
			throw new System.NotImplementedException();
		}

		public override void Destroy()
		{
			throw new System.NotImplementedException();
		}
		
		#endregion
	}
}