using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace butterflowersOS.AI
{
	[Serializable]
	[PostProcess(typeof(GrayscaleRenderer), PostProcessEvent.AfterStack, "Custom/Grayscale")]
	public sealed class Grayscale : PostProcessEffectSettings
	{
		[Range(0f, 1f), Tooltip("Bluelite intensity.")] public FloatParameter blend = new FloatParameter { value = 0.5f };
		[Range(0f, 1f), Tooltip("Aggregate intensity.")] public FloatParameter intensity = new FloatParameter { value = 0.5f };
		
		public FloatParameter offset = new FloatParameter { value = 0.5f };
		public ColorParameter color = new ColorParameter {value = Color.white};
		public FloatParameter tiling = new FloatParameter{value = 1f};
	}
 
	public sealed class GrayscaleRenderer : PostProcessEffectRenderer<Grayscale>
	{
		public override void Render(PostProcessRenderContext context)
		{
			var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Grayscale"));
			sheet.properties.SetFloat("_Blend", settings.blend);
			sheet.properties.SetFloat("_Intensity", settings.intensity);
			sheet.properties.SetFloat("_Offset", settings.offset);
			sheet.properties.SetColor("_Color", settings.color);
			sheet.properties.SetFloat("_Tiling", settings.tiling);
			context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
		}
	}
}