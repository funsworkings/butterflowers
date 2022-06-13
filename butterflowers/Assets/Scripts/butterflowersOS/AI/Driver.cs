using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.AI.Objects;
using butterflowersOS.Data;
using butterflowersOS.Presets;
using Neue.Agent.Brain.Data;
using Neue.Reference.Types;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using uwu.Extensions;
using Random = UnityEngine.Random;

namespace butterflowersOS.AI
{
	public class Driver : MonoBehaviour
	{
		// Collections
		
		List<SurveillanceData> allLogs = new List<SurveillanceData>();
		
		List<Entity> activeEntities = new List<Entity>();
		List<Entity> inactiveEntities = new List<Entity>();

		List<Point> points = new List<Point>();

		// Properties

		[SerializeField] WorldPreset preset;
		[SerializeField] VFXER _pool;

		[SerializeField] SurveillanceData log;
		[SerializeField] SurveillanceLogData[] captures = new SurveillanceLogData[]{};
		[SerializeField] SurveillanceLogData capture = null;

		// Attributes

		[SerializeField] float refreshTime = 1f;
		[SerializeField] bool readEvents = false;
		[SerializeField] int eventStackHeight = 5;
		
		[Header("Scene elements")]
		[SerializeField] Box nest = null;
		[SerializeField] float minNestKick = 1f, maxNestKick = 10f;

		[Header("Scene attributes")] 
		[SerializeField] Vector4 baseline;
		[SerializeField, Range(0f, 1f)] float saturation = 0f, value = 0f;
		[SerializeField, Range(0f, 1f)] float maxSaturationSpread = 1f, maxValueSpread = 1f;
		[SerializeField] int tileX, tileY;
		[SerializeField] float tileSX, tileSY;
		[SerializeField] bool useLogForColorValues = true;
		[SerializeField] int numberOfPlayerTextures, numberOfWorldTextures;
		[SerializeField] Texture2D playerTexture;

		float refresh_t = 0f;
		int logIndex = 0;
		
		#region Accessors

		public List<Entity> ActiveEntities => activeEntities;
		public List<Point> EventStack => points;

		#endregion
		

		public void Initialize(Profile profile, SurveillanceData[] data, Texture2D image, int dimensionX, int dimensionY)
		{
			baseline = new Vector4(profile.GetWeight(Frame.Destruction), profile.GetWeight(Frame.Nurture), profile.GetWeight(Frame.Order), 1f - profile.GetWeight(Frame.Quiet));
			
			tileX = dimensionX;
			tileSX = 1f / tileX;
			tileY = dimensionY;
			tileSY = 1f / tileY;
			
			numberOfPlayerTextures = dimensionX * dimensionY;
			numberOfWorldTextures = preset.worldTextures.Length;

			playerTexture = image;
			
			allLogs.AddRange(data);
			if (allLogs.Count > 0) {
				StopCoroutine("Loop");
				StartCoroutine("Loop");
			}
		}

		void Start()
		{
			var entities = FindObjectsOfType<Entity>();
			foreach (Entity e in entities) 
			{
				if(e.isActiveAndEnabled) EnableEntity(e);
				else DisableEntity(e);
			}
			
			Entity.OnEnabled += EnableEntity;
			Entity.OnDisabled += DisableEntity;
			Entity.OnDisposed += DisposeEntity;
		}

		void OnDestroy()
	    {
		    StopAllCoroutines();
	    }
		
		#region Entities

		void EnableEntity(Entity e)
		{
			if(!activeEntities.Contains(e)) activeEntities.Add(e);
			if (inactiveEntities.Contains(e)) inactiveEntities.Remove(e);
		}

		void DisableEntity(Entity e)
		{
			if(!inactiveEntities.Contains(e)) inactiveEntities.Add(e);
			if (activeEntities.Contains(e)) activeEntities.Remove(e);
		}

		void DisposeEntity(Entity e)
		{
			if(activeEntities.Contains(e)) activeEntities.Remove(e);
			if(inactiveEntities.Contains(e)) inactiveEntities.Remove(e);
		}
		
		#endregion
	    
	    #region Events

	    EVENTCODE[] ScrapeEventLogs(SurveillanceLogData log)
	    {
		    List<EVENTCODE> eventStack = new List<EVENTCODE>();

		    if (log != null) 
		    {
			    foreach (sbyte e in log.events)
				    eventStack.Add((EVENTCODE) e);
		    }

		    return eventStack.ToArray();
	    }

	    IEnumerator Loop()
	    {
		    log = null;
		    captures = new SurveillanceLogData[]{};
		    capture = null;
		    
		    logIndex = 0;
		    
		    float t = 0f;
		    float i = 0f;

		    int captureIndex = 0;
		    int captureStackHeight = 0;
		    
		    while (true) 
		    {
			    log = allLogs[logIndex];
			    captures = log.logs;
			    capture = null;
			    
			    captureIndex = 0;
			    captureStackHeight = captures.Length;

			    if (captureStackHeight > 0) 
			    {
				    while (captureIndex <= (captureStackHeight - 1)) 
				    {
					    t = 0f;
					    i = (refreshTime / captures.Length);
					    //i = refreshTime;
					    
					    capture = captures[captureIndex];
					    HandleLog(capture, captureIndex, i);

					    while (t < i) 
					    {
						    t += Time.deltaTime;
						    yield return null;
					    }

						captureIndex++;
					    yield return null;
				    }
			    }

			    if (++logIndex > (allLogs.Count - 1)) logIndex = 0; // Increment active log
			    yield return null;
		    }
	    }
	    
	    #endregion
	    
	    #region Ops

	    void HandleLog(SurveillanceLogData log, int index, float interval)
	    {
		    var events = ScrapeEventLogs(log);
		    Debug.LogFormat("Handle capture #{0} with {1} events", index, events.Length);

		    // Set nest fill attributes
		    if (useLogForColorValues) 
		    {
			    saturation = log.nestFill / 255f * maxSaturationSpread;
			    value = log.butterflyHealth / 255f * maxValueSpread;
		    }

		    if(readEvents) StopCoroutine("LoopEventsFromLog");

		    readEvents = true;
		    StartCoroutine(LoopEventsFromLog(log, events, interval));
	    }

	    void HandleEvent(EVENTCODE @event, SurveillanceLogData log)
	    {
		    Debug.LogFormat("Handle event => {0}", @event);

		    Entity entity = null;
		    bool fallbackEntityTrigger = true;

		    switch (@event) 
		    {
				case EVENTCODE.NESTKICK:

					float nestKick = Random.Range(minNestKick, maxNestKick);
					nest.Propel(nestKick);
					break;
				
				case EVENTCODE.BEACONACTIVATE:
					entity = SpawnEntity("beacon_add");
					break;
				case EVENTCODE.DISCOVERY:
					entity = SpawnEntity("discovery");
					break;
				case EVENTCODE.NESTSPILL:
					entity = SpawnEntity("fireworks");
					break;
				case EVENTCODE.BEACONPLANT:
					entity = SpawnEntity("quad");

					float ox = Random.Range(0, tileX) * tileSX;
					float oy = Random.Range(0, tileY) * tileSY;

					Texture2D targetTexture = playerTexture;
					int randomTexture = Random.Range(0, numberOfPlayerTextures + Mathf.FloorToInt(numberOfPlayerTextures * preset.worldTextureProbability));
					
					bool usePlayerTexture = randomTexture < numberOfPlayerTextures;
					if (!usePlayerTexture) 
					{
						targetTexture = preset.worldTextures[Random.Range(0, numberOfWorldTextures)];
						Debug.LogWarningFormat("Import custom external file: {0}", targetTexture.name);
					}
					
					entity.Trigger(saturation, value, baseline, new object[] { ox, oy, targetTexture, usePlayerTexture });
					
					fallbackEntityTrigger = false; // Ignore fallback
					break;
				case EVENTCODE.BEACONFLOWER:
					entity = SpawnEntity("flora");
					break;
				default:
					break; // Default state
		    }

		    if (fallbackEntityTrigger && entity != null) 
		    {
				entity.Trigger(saturation, value, baseline); // Trigger entity drawing with values    
		    }

		    if(points.Count >= eventStackHeight)
		    {
				points.RemoveAt(0); // Pop first element to free space
		    }
		    points.Add(new Point(@event, nest.transform.position));
	    }
	    
	    #region Event ops

	    Entity SpawnEntity(string id)
	    {
		    GameObject instance = _pool.RequestEntity(id);
		    instance.transform.position = nest.transform.position;
		    
		    return instance.GetComponent<Entity>();
	    }

	    #endregion

	    IEnumerator LoopEventsFromLog(SurveillanceLogData log, EVENTCODE[] events, float duration)
	    {
		    int eHeight = events.Length;
		    
		    EVENTCODE @event;
		    int e = 0;
		    float t = 0f;

		    if (eHeight > 0) 
		    {
			    float i = (duration / eHeight);
			    
			    while (e < eHeight) 
			    {
				    t = 0f;
				    @event = events[e];
				    
				    HandleEvent(@event, log);

				    while (t < i) 
				    {
					    t += Time.deltaTime;
					    yield return null;
				    }

				    e++;
				    yield return null;
			    }
		    }

		    readEvents = false;
	    }
	    
	    #endregion
	}
}