using Neue.Agent.Actions.Movement;
using UnityEngine;
using uwu.Extensions;

namespace Neue.Agent.Actions
{
	public class Motion : MonoBehaviour
	{
		// Properties

		Animator animator;
		Navigation navigation;

		float movementBlend = 0f;
		
		#region Accessors

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

		void Update()
		{
			//animator.SetLayerWeight(turnLayer, navigation.Turn);
			
			float t_movementBlend = Mathf.Pow(navigation.Move, 2f) * 2f;
			if (t_movementBlend <= 0f) movementBlend *= .95f;
			else movementBlend = t_movementBlend;
			
			animator.SetFloat(moveBlendParam, movementBlend);

			animator.SetFloat(turnBlendParam, Mathf.Sign(navigation.Bearing));
			print(navigation.Bearing);
		}
	}
}