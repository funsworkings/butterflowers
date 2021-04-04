using butterflowersOS.AI.Objects;
using UnityEngine;

namespace butterflowersOS.AI
{
	public abstract class Shape : Entity
	{
		[SerializeField] protected Color color = Color.white;
		[SerializeField] bool randomizeColor = true;

		protected override void OnEnable()
		{
			base.OnEnable();
			
			if(randomizeColor) color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			transform.localEulerAngles = Vector3.zero;
		}

		static Material lineMaterial;
		static void CreateLineMaterial()
		{
			if (!lineMaterial)
			{
				// Unity has a built-in shader that is useful for drawing
				// simple colored things.
				Shader shader = Shader.Find("Hidden/Internal-Colored");
				lineMaterial = new Material(shader);
				lineMaterial.hideFlags = HideFlags.HideAndDontSave;
				// Turn on alpha blending
				lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				// Turn backface culling off
				lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
				// Turn off depth writes
				lineMaterial.SetInt("_ZWrite", 0);
			}
		}

		// Will be called after all regular rendering is done
		public virtual void OnRenderObject()
		{
			if (WillRenderWithCamera(Camera.current)) {

				CreateLineMaterial();
				// Apply the line material
				lineMaterial.SetPass(0);

				DidRenderShape();
			}
		}

		protected abstract void DidRenderShape();

		protected virtual bool WillRenderWithCamera(Camera camera)
		{
			var mask = camera.cullingMask;
			return mask == (mask | (1 << gameObject.layer));
		}
	}
}