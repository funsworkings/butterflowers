using System;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Data;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using uwu.Extensions;
using Random = System.Random;

namespace butterflowersOS.AI
{
	public class RemoteAgent : MonoBehaviour
	{
		// Collections

		[SerializeField] SurveillanceLogData[] logs;
		
		// Properties

		bool load = false;

		[SerializeField] SurveillanceLogData log;
		[SerializeField] EventLog events = null;
		[SerializeField] ParticleSystem ps = null;
		[SerializeField] PostProcessVolume postprocessing = null;

		// Attributes

		[SerializeField] float refreshTime = 1f;
		
		[Header("Scene elements")]
		[SerializeField] MeshRenderer lightRenderer = null;
		[SerializeField] float minLightOpacity = 0f;
		[SerializeField] float maxLightOpacity = .5f;
		[SerializeField] float lightLerpSpeed = 1f;
		new float light;
		float t_light;
		
		[Header("Particle systems")]
		[SerializeField] float minEmission = 0f;
		[SerializeField] float maxEmission = 666f;
		[SerializeField] float emissionLerpSpeed = 1f;
		float emission, t_emission;
		
		[Header("Postprocessing")]
		[SerializeField] float minSaturation = 0f;
		[SerializeField] float maxSaturation = 100f;
		[SerializeField] float saturationLerpSpeed = 1f;
		[SerializeField] float minBloom = 0f;
		[SerializeField] float maxBloom = 1f;
		[SerializeField] float bloomLerpSpeed = 1f;

		[Header("Debug")] 
		[SerializeField, Range(-1f, 1f)] float debugButterflowerHealth = -1f;
		[SerializeField, Range(-1f, 1f)] float debugNestFill = -1f;
		[SerializeField] bool debugEvents = false;
		[SerializeField] int debugMinEventStack = 1, debugMaxEventStack = 10;

		
		float saturation, t_saturation;
		float bloom, t_bloom;

		
		float refresh_t = 0f;
		int logIndex = 0;

		public void Initialize(SurveillanceData[] data)
		{
			List<SurveillanceLogData> logs = new List<SurveillanceLogData>();
			foreach (SurveillanceData dat in data) 
			{
				logs.AddRange(dat.logs);
			}

			this.logs = logs.ToArray();
			logIndex = 0;
			
			if(logs.Count > 0)
				OnUpdatedLog();
			
			emission = t_emission;
			saturation = t_saturation;
			bloom = t_bloom;

			load = true;
		}
		
	    void Update()
	    {
		    if (!load) return;
		    if (logs.Length == 0) return;

		    refresh_t += Time.deltaTime;
		    if (refresh_t > refreshTime) 
		    {
			    if (++logIndex >= logs.Length) logIndex = 0;
			    refresh_t = 0f;
			    
			    OnUpdatedLog();
		    }
		    log = logs[logIndex];
		    
		    LerpParticleEmission();
		    LerpPostProcessing();
		    
		    /*
		    ApplyParticleEmission();
		    ApplyPostProcessing();

		    if (lightRenderer.gameObject.activeInHierarchy) 
		    {
			    LerpLight();
			    ApplyLight();
		    }
		    */
	    }

	    void OnUpdatedLog()
	    {
		    float b = (debugButterflowerHealth >= 0f) ? debugButterflowerHealth*255f : log.butterflyHealth;
		    float n = (debugNestFill >= 0f) ? debugNestFill*255f : log.nestFill;
		    
		    var butterfly = (b / 255f);
		    t_emission = butterfly.RemapNRB(0f, 1f, minEmission, maxEmission);
		    t_light = butterfly.RemapNRB(0f, 1f, minLightOpacity, maxLightOpacity);

		    var nest = (n / 255f);
				t_saturation = nest.RemapNRB(0f, 1f, minSaturation, maxSaturation);
				t_bloom = nest.RemapNRB(0f, 1f, minBloom, maxBloom);
				
		    PushAllEventLogs();
	    }
	    
	    #region Particle systems

	    void LerpParticleEmission()
	    {
		    emission = Mathf.Lerp(emission, t_emission, Time.deltaTime * emissionLerpSpeed);
	    }

	    void ApplyParticleEmission()
	    {
		    var emissionModule = ps.emission;
				emissionModule.rateOverTimeMultiplier = emission;
	    }
	    
	    #endregion
	    
	    #region Postprocessing

	    void LerpPostProcessing()
	    {
		    saturation = Mathf.Lerp(saturation, t_saturation, Time.deltaTime * saturationLerpSpeed);
		    bloom = Mathf.Lerp(bloom, t_bloom, Time.deltaTime * bloomLerpSpeed);
	    }

	    void ApplyPostProcessing()
	    {
		    ColorGrading grading = null;
		    Bloom bloom = null;

		    if (postprocessing.profile.TryGetSettings(out grading)) 
		    {
			    grading.saturation.value = saturation; // Adjust saturation in color grading
		    }

		    if (postprocessing.profile.TryGetSettings(out bloom)) 
		    {
			    bloom.intensity.value = this.bloom; // Adjust bloom intensity
		    }
	    }
	    
	    #endregion
	    
	    #region Scene elements
	    
	    void LerpLight()
	    {
		    light = Mathf.Lerp(light, t_light, Time.deltaTime * lightLerpSpeed);
	    }

	    void ApplyLight()
	    {
		    print("a[ply loight");
		    var color = lightRenderer.material.GetColor("_TintColor");
		    lightRenderer.material.SetColor("_TintColor", Extensions.SetOpacity(light, color));
	    }
	    
	    #endregion
	    
	    #region Events

	    void PushAllEventLogs()
	    {
		    if (log == null) return;
		    
		    List<EVENTCODE> eventStack = new List<EVENTCODE>();

		    if (!debugEvents) {
			    foreach (sbyte e in log.events) eventStack.Add((EVENTCODE) e);
		    }
		    else 
		    {
			    int eventCount = UnityEngine.Random.Range(debugMinEventStack, debugMaxEventStack);
			    for (int i = 0; i < eventCount; i++) 
			    {
				    var vals = System.Enum.GetValues(typeof(EVENTCODE));
				    var val = vals.GetValue(UnityEngine.Random.Range(0, vals.Length));
				    
				    eventStack.Add((EVENTCODE)val);
			    }
		    }

		    events.Push(eventStack.ToArray());
	    }
	    
	    #endregion
	}
}