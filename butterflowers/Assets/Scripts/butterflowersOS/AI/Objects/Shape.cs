using UnityEngine;

namespace butterflowersOS.AI.Objects
{
	public abstract class Shape : Entity
	{
		[SerializeField] protected Color color = Color.white;
		[SerializeField] ColorMode colorMode = ColorMode.Default;


		public enum ColorMode
		{
			Default,
			Random,
			Baseline,
			RandomFromBaseline
		}
		

		protected override void OnEnable()
		{
			base.OnEnable();
			
			transform.localEulerAngles = Vector3.zero;

			if (colorMode == ColorMode.Random) 
			{
				color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
			}
		}

		public override void Trigger(float saturation, float value, Color baseline, params object[] data)
		{
			base.Trigger(saturation, value, baseline);

			if (colorMode == ColorMode.Baseline) 
			{
				color = baseline;
			}
			else if (colorMode == ColorMode.RandomFromBaseline) 
			{
				color = ShapeExtensions.RandomColorFromRange(baseline, saturation, value);
			}
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
			if (!ready) return; // Ignore draw calls when not initialized
			
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

	public static class ShapeExtensions
	{
		public static Color RandomColorFromRange(Color color, float sRange, float hRange)
		{
			Color.RGBToHSV(color, out float h, out float s, out float v);

			h = Mathf.Repeat(h + Random.Range(0f, sRange), 1f);
			s = Mathf.Repeat(s + Random.Range(0f, hRange), 1f);
			
			Color resultant = Color.HSVToRGB(h, s, v);
			resultant.a = color.a;
			
			return resultant;
		}
	}
}