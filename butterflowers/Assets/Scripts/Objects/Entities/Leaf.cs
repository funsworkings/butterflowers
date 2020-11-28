using Settings;
using UnityEngine;
using uwu.Extensions;

namespace Objects.Entities
{
	public class Leaf : MonoBehaviour
	{
		[SerializeField] WorldPreset preset;
		[SerializeField] Transform mesh;
		
		Vector3 scale = Vector3.zero;
		[SerializeField] bool visible = false, awake = false;
		
		bool lerp = false;
		float lerp_t = 0f, lerp_duration = 3f;

		Animator animator;

		public int index;
		public float progress;
		public float offset;

		public bool Visible => visible;

		public float Scale
		{
			get
			{
				return scale.x;
			}
			set
			{
				scale = Vector3.one * value;
			}
		}

		#region Monobehaviour callbacks

		void Awake()
		{
			scale = transform.localScale;
			
			transform.localScale = Vector3.zero;
			visible = false;

			animator = GetComponent<Animator>();
		}

		void Start()
		{
			lerp_duration = preset.leafGrowTime;
		}

		void Update()
		{
			if (lerp) {
				lerp_t += Time.deltaTime;
				
				float i = Mathf.Clamp01(lerp_t / lerp_duration);
				if (i >= 1f)
					lerp = false;
				
				UpdateSize(i, visible);
			}	
		}
		
		#endregion
		
		#region Show/hide

		public void Show(bool immediate)
		{
			visible = true;
			
			lerp = !immediate;
			lerp_t = (lerp) ? 0f : lerp_duration;

			float i = Mathf.Clamp01(lerp_t / lerp_duration);
			UpdateSize(i, true);
		}

		public void Hide(bool immediate)
		{
			visible = false;
			
			lerp = !immediate;
			lerp_t = (lerp) ? 0f : lerp_duration;

			float i = Mathf.Clamp01(lerp_t / lerp_duration);
			UpdateSize(i, false);
		}

		void UpdateSize(float interval, bool shown)
		{
			Vector3 a = (shown) ? Vector3.zero : scale;
			Vector3 b = (shown) ? scale : Vector3.zero;

			Vector3 dir = (b - a);

			if (shown)
				interval = preset.leafGrowCurve.Evaluate(interval);

			transform.localScale = a + (dir)*interval;
		}
		
		#endregion

		#region Fluttering

		public void Flutter()
		{
			animator.SetBool("awake", true);
		}

		public void Unflutter()
		{
			animator.SetBool("awake", false);
		}

		#endregion
	}
}