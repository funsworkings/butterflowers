using UnityEngine;

namespace butterflowersOS.AI.Objects
{
	public class Image : Entity
	{
		// Properties

		MeshRenderer renderer;
		Material material;

		Vector3 axis;
		Vector3 scale;

		Vector2 textureScale;

		protected override void Awake()
		{
			base.Awake();
			
			renderer = GetComponent<MeshRenderer>();
			material = renderer.material;
			textureScale = material.mainTextureScale;
			
			scale = transform.localScale;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			float minScale = scale.x;
			//transform.localScale = Vector3.one * Random.Range(minScale, minScale * 2f);
		}

		protected override void Update()
		{
			base.Update();

			transform.position += (Vector3.down * Time.deltaTime * 1f);
			transform.Rotate(axis, Time.deltaTime * 16f, Space.World);
		}

		public override void Trigger(float saturation, float value, Color baseline, params object[] data)
		{
			base.Trigger(saturation, value, baseline, data);

			bool usePlayerTexture = (bool) data[3];

			material.mainTexture = (Texture2D) data[2];
			material.mainTextureScale = (usePlayerTexture) ? textureScale : Vector2.one;
			material.mainTextureOffset = (usePlayerTexture)?  new Vector2((float)data[0], (float)data[1]) : Vector2.zero;
			
			axis = Random.insideUnitSphere;
		}

		protected override void DidOrientToCamera(Vector3 direction)
		{
			transform.forward = -direction;
		}
	}
}