using UnityEngine;

namespace butterflowersOS.AI
{
	public class EventSnake : MonoBehaviour
	{
		#region Internal

		public enum State
		{
			Move,
			Turn
		}
		
		#endregion
		
		// Properties

		public State _state = State.Move;

		[Header("Collisions")] 
			[SerializeField] LayerMask collisionMask;
			[SerializeField] float collisionDistance = 1f;

			Ray collisionRay;
			RaycastHit collisionHit;
		
		// Attributes

		[Header("Movement")] 
			[SerializeField] float baselineVelocity = 1f;
			[SerializeField] float velocitySpeed = 1f;
			[SerializeField] float accelerationPerSegment = 1f;
			[SerializeField] float acceleration = 0f;
			[SerializeField] float deceleration = 1f;
			[SerializeField] float rotationSpeed = 1f;
			[SerializeField] float rotationSnapThreshold = 1f;
			[SerializeField, Range(0f, 1f)] float changeDirectionProbability = .5f;


		float _velocity = 0f, t_velocity = 0f;
		Quaternion bearing;

		void Start()
		{
			var t = transform;
			
			Vector3 angle = t.localEulerAngles;
			t.localEulerAngles = new Vector3(Mathf.RoundToInt(angle.x / 90f) * 90f,  // Clamp angle to 90deg intervals
											 Mathf.RoundToInt(angle.y / 90f) * 90f,
											 Mathf.RoundToInt(angle.z / 90f) * 90f);

			bearing = t.localRotation;
			_velocity = t_velocity = baselineVelocity;
		}

		void Update()
		{
			Transform t = transform;
			
			if (_state == State.Move) 
			{
				if (DetectCollision()) 
				{
					bearing = t.localRotation * Quaternion.AngleAxis(90f, t.right);
					_state = State.Turn;
				}
				else 
				{
					t.localPosition += (t.forward * Time.deltaTime * _velocity);
				}
				
				Decelerate();
				
				t_velocity = baselineVelocity + acceleration;
				SmoothVelocity();
			}
			else // Turn 
			{
				bool @continue = Turn();
				if (!@continue) _state = State.Move;
			}
		}	
		
		#region Movement

		void Accelerate(float a)
		{
			if (_state == State.Turn) 
			{
				SnapToBearing();
				_state = State.Move;
			}

			acceleration += a;
		}

		void Decelerate()
		{
			acceleration *= (1f - (Time.deltaTime * deceleration));
		}

		void SmoothVelocity()
		{
			_velocity = Mathf.Lerp(_velocity, t_velocity, Time.deltaTime * velocitySpeed);
		}

		bool Turn()
		{
			float angle = Quaternion.Angle(transform.localRotation, bearing);
			if (angle < rotationSnapThreshold) 
			{
				SnapToBearing();
				return false;
			}

			transform.localRotation = Quaternion.RotateTowards(transform.localRotation, bearing, Time.deltaTime * rotationSpeed);
			return true;
		}

		void SnapToBearing()
		{
			transform.localRotation = bearing;
		}
		
		#endregion
		
		
		#region Collisions

		bool DetectCollision()
		{
			var t = transform;
			
			collisionRay = new Ray(t.position, t.forward);
			collisionHit = new RaycastHit();

			return Physics.Raycast(collisionRay, out collisionHit, collisionDistance, collisionMask.value);
		}
		
		#endregion
		
		#region Events

		public void Push(EVENTCODE[] events)
		{
			float _acceleration = (events.Length * accelerationPerSegment);
			Accelerate(_acceleration);
		}
		
		#endregion
	}
}