using System;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Data;
using UnityEngine;

namespace butterflowersOS.AI
{
	public class RemoteCursor : MonoBehaviour
	{
		// Collections

		[SerializeField] Vector2[] velocities;
		
		// Properties

		bool load = false;

		RectTransform rect;

		[SerializeField] Vector2 velocity, t_velocity;

		// Attributes

		[SerializeField] float refreshTime = 1f;
		[SerializeField] float velocityLerpSpeed = 1f;
		
		float refresh_t = 0f;
		int logIndex = 0;

		void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		public void Initialize(SurveillanceData[] data)
		{
			List<SurveillanceLogData> logs = new List<SurveillanceLogData>();
			foreach (SurveillanceData dat in data) 
			{
				logs.AddRange(dat.logs);
			}

			velocities = logs.Select(log => new Vector2(log.cursorX, log.cursorY)).ToArray();
			logIndex = 0;

			load = true;
		}
		
	    void Update()
	    {
		    if (!load) return;

		    refresh_t += Time.deltaTime;
		    if (refresh_t > refreshTime) 
		    {
			    if (++logIndex >= velocities.Length) logIndex = 0;
			    refresh_t = 0f;
		    }
		    t_velocity = velocities[logIndex];
		    
		    LerpVelocity();
		    MoveCursor();
	    }
	    
	    
	    #region Velocity

	    void LerpVelocity()
	    {
		    velocity = Vector2.Lerp(velocity, t_velocity, Time.deltaTime * velocityLerpSpeed);
	    }
	    
	    #endregion
	    
	    #region Movement

	    void MoveCursor()
	    {
		    rect.anchoredPosition += velocity * Time.deltaTime;
	    }
	    
	    #endregion
	}
}