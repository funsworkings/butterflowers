using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace butterflowersOS.AI
{
	[Serializable]
	[PostProcess(typeof(SmileRenderer), PostProcessEvent.AfterStack, "Custom/Smiley")]
	public sealed class Smiley : PostProcessEffectSettings
	{
		public TextureParameter _gradient = new TextureParameter() {value = null};
		public TextureParameter _heatmap = new TextureParameter() {value = null};
		
		public FloatParameter _blend = new FloatParameter { value = 0f };
		public IntParameter _steps = new IntParameter() {value = 128};

		/*
		public FloatParameter offset = new FloatParameter { value = 0.5f };
		public ColorParameter color = new ColorParameter {value = Color.white};
		public FloatParameter tiling = new FloatParameter { value = 1f };
		*/
	}
 
	public sealed class SmileRenderer : PostProcessEffectRenderer<Smiley>
	{
		public override void Render(PostProcessRenderContext context)
		{
			var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Smiley"));
			
			sheet.properties.SetTexture("_Gradient", settings._gradient);
			sheet.properties.SetTexture("_Heatmap", settings._heatmap);
			
			sheet.properties.SetFloat("_Blend", settings._blend);
			sheet.properties.SetInt("_Steps", settings._steps);
			
			/*
			sheet.properties.SetFloat("_Offset", settings.offset);
			sheet.properties.SetFloat("_Tiling", settings.tiling);
			sheet.properties.SetColor("_Color", settings.color);*/
			
			context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
		}
	}
}