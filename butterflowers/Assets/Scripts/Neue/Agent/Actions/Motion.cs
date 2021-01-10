using Neue.Agent.Actions.Movement;
using Neue.Agent.Types;
using UnityEngine;

namespace Neue.Agent.Actions
{
	public class Motion : Module
	{
		// Properties

		[SerializeField] bool moving = false;

		Animator animator;
		Navigation navigation;

		float movementBlend = 0f;
		
		#region Accessors

		int baseLayer => 0;
		int moveLayer => 1;
		int turnLayer => 2;

		string moveBlendParam => "move_blend";
		string turnBlendParam => "turn_blend";
		
		#endregion

		void Awake()
		{
			animator = GetComponent<Animator>();
			navigation = GetComponent<Navigation>();
		}

		#region Module

		public override void Continue()
		{
			UpdateMovementBlends();

			moving = (movementBlend > .01f);
			SetLayerWeightsFromState();
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

		#region Animations

		void UpdateMovementBlends()
		{
			float t_movementBlend = Mathf.Pow(navigation.Move, 2f) * 2f;
			if (t_movementBlend <= 0f) movementBlend *= .95f;
			else movementBlend = t_movementBlend;
			
			animator.SetFloat(moveBlendParam, movementBlend);
			animator.SetFloat(turnBlendParam, Mathf.Sign(navigation.Bearing));
		}

		void SetLayerWeightsFromState()
		{
			float baseWeight = 1f;
			float movementWeight = 0f;

			if (moving) movementWeight = 1f;
			
			animator.SetLayerWeight(baseLayer, baseWeight);
			animator.SetLayerWeight(moveLayer, movementWeight);
		}
		
		#endregion
	}
}