using UnityEngine;

namespace uwu.UI.Behaviors.Visibility
{
	public class ToggleAnimation : ToggleVisibility
	{
		[SerializeField] Animator animator;
		
		[Header("Animation parameters")] 
			[SerializeField] string showTrigger;
			[SerializeField] string hideTrigger;
			[SerializeField] string visibleBoolean;

		void Awake()
		{
			if (animator == null)
				animator = GetComponent<Animator>();
		}
		
		protected override void SetTarget(bool isVisible)
		{
			
		}

		protected override void SetCurrent(bool isVisible)
		{
			
		}

		protected override void SetTargetToCurrent()
		{
			
		}

		protected override void SetCurrentToTarget()
		{
			
		}
	}
}