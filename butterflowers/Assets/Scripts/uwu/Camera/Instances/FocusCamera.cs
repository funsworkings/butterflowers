using UnityEngine;

namespace uwu.Camera.Instances
{
	public class FocusCamera : GameCamera
	{
		[SerializeField] Transform focus;

		[SerializeField] bool follow = true, lookAt = true;
		Transform defaultFocus;

		protected override void Start()
		{
			base.Start();
			defaultFocus = focus;
		}

		public override void Disable()
		{
			base.Disable();
			LoseFocus();
		}

		public void Focus(Transform target)
		{
			focus = target;

			if (follow) camera.Follow = focus;
			if (lookAt) camera.LookAt = focus;
		}

		public void LoseFocus()
		{
			focus = defaultFocus;
		}
	}
}