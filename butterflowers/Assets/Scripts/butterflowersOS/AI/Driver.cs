using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.AI.Objects;
using butterflowersOS.Data;
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
		
		// Properties

		[SerializeField] SurveillanceData log;
		[SerializeField] SurveillanceLogData[] captures = new SurveillanceLogData[]{};
		[SerializeField] SurveillanceLogData capture = null;

		// Attributes

		[SerializeField] float refreshTime = 1f;
		[SerializeField] bool readEvents = false;
		
		[Header("Scene elements")]
		[SerializeField] Box nest = null;
		[SerializeField] float minNestKick = 1f, maxNestKick = 10f;


		float refresh_t = 0f;
		int logIndex = 0;
		
		#region Accessors

		public List<Entity> ActiveEntities => activeEntities;

		#endregion
		

		public void Initialize(SurveillanceData[] data)
		{
			allLogs.AddRange(data);
			if(allLogs.Count > 0) StartCoroutine("Loop");
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
		    
		    if(readEvents) StopCoroutine("LoopEventsFromLog");

		    readEvents = true;
		    StartCoroutine(LoopEventsFromLog(events, interval));
	    }

	    void HandleEvent(EVENTCODE @event)
	    {
		    Debug.LogFormat("Handle event => {0}", @event);

		    switch (@event) 
		    {
				case EVENTCODE.NESTKICK:

					float nestKick = Random.Range(minNestKick, maxNestKick);
					nest.Propel(nestKick);
					break;
		    }
	    }

	    IEnumerator LoopEventsFromLog(EVENTCODE[] events, float duration)
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
				    
				    HandleEvent(@event);

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