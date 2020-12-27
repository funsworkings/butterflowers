using System;
using Noder.Graphs;
using Noder.Nodes.Behaviours.Core;
using UnityEngine;
using uwu.Camera.Instances;
using uwu.Extensions;

namespace Noder.Nodes.Behaviours.Events
{
	[Obsolete("Obsolete API!", true)]
	public class MoveToAgent : BaseEventNode<Focusable>
	{
		public bool escape = false;

		Camera cam;

		protected override void OnEnter()
		{
			if (cam == null) cam = Camera.main;
			
			base.OnEnter();
		}

		protected override bool FireEvent()
		{
			var tree = (graph as ModuleTree);
			if (tree != null) {

				var agent = GetInputValue<Focusable>("data", this.data);

				if (agent == null || escape) 
				{
					if (World.Instance.IsFocused) {
						ModuleTree.onReceiveEvent(tree, EVENTCODE.REFOCUS, null, rewards);
						return true;
					}
				}
				else 
				{
					if (!agent.isFocused) {
						var pos = Vector2.zero;
						var visible = agent.transform.IsVisible(cam, out pos);

						if (visible) {
							ModuleTree.onReceiveEvent(tree, EVENTCODE.REFOCUS, agent, rewards);
							return true;
						}

						return false;
					}
					else {
						Debug.LogWarning("Attempting to move to a non-focusable AGENT in node!");
					}
				}
			}

			return false;
		}
	}
}