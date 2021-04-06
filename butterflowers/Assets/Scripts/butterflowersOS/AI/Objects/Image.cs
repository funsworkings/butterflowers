using UnityEngine;

namespace butterflowersOS.AI.Objects
{
	public class Image : Entity
	{
		// Properties

		MeshRenderer renderer;
		Material material;


		protected override void Awake()
		{
			base.Awake();
			
			renderer = GetComponent<MeshRenderer>();
			material = renderer.material;
		}

		public override void Trigger(float saturation, float value, Color baseline, params object[] data)
		{
			base.Trigger(saturation, value, baseline, data);
			
			material.mainTextureOffset = new Vector2((float)data[0], (float)data[1]);
		}

		protected override void DidOrientToCamera(Vector3 direction)
		{
			transform.forward = -direction;
		}
	}
}