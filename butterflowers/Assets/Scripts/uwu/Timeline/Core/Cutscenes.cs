﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace uwu.Timeline.Core
{
	[RequireComponent(typeof(PlayableDirector))]
	public class Cutscenes : MonoBehaviour
	{
		// Events

		public System.Action<PlayableAsset> Started, Completed;
		
		
		#region Internal

		public enum State
		{
			Playing,
			Paused,
			Stopped
		}

		#endregion

		#region Properties

		PlayableDirector playableDirector;
		float cache_t = 0f;

		#endregion

		#region Attributes

		State state = State.Stopped;
		double lastTimestamp = 0.0;

		[SerializeField] PlayableAsset m_cutscene;

		[Header("Debug")] 
			[SerializeField] bool debugTriggerPlay = false;
			[SerializeField] bool debugTriggerCancel = false;

		#endregion

		#region Accessors

		public bool playing => state == State.Playing;
		public bool paused => state == State.Paused;
		public bool stopped => state == State.Stopped;

		public PlayableAsset cutscene
		{
			get => m_cutscene;
			set
			{
				if (!stopped)
					Stop();

				m_cutscene = value;
				playableDirector.playableAsset = cutscene;
			}
		}

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			playableDirector = GetComponent<PlayableDirector>();
		}

		void OnEnable()
		{
			playableDirector.played += onPlay;
			playableDirector.stopped += onStop;

			m_cutscene = playableDirector.playableAsset;
		}

		void OnDisable()
		{
			playableDirector.played -= onPlay;
			playableDirector.stopped -= onStop;
		}

		void Start()
		{
			if(cutscene != null) Play(cutscene);
		}

		void Update()
		{
			if (playing) 
			{
				lastTimestamp = playableDirector.time; // Record last valid timestamp
			}

			if (debugTriggerPlay) 
			{
				Play(cutscene);
				debugTriggerPlay = false;
			}

			if (debugTriggerCancel || (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.X))) 
			{
				Cancel();	
				debugTriggerCancel = false;
			}
		}

		#endregion

		#region Playable director callbacks

		void onPlay(PlayableDirector director)
		{
			state = State.Playing;
			lastTimestamp = director.time;
			
			Started?.Invoke(director.playableAsset);
		}

		void onStop(PlayableDirector director)
		{
			state = State.Stopped;

			Debug.LogFormat("{0} completed at time=> {1}  duration=> {2}", director.playableAsset.name, lastTimestamp, director.duration);
			//if (lastTimestamp >= director.duration) // Detect completion
			//{
				if (Completed != null)
					Completed(director.playableAsset);
			//}
		}

		#endregion

		#region Operations

		public void Play()
		{
			if (!playing) 
			{
				if (paused) 
				{
					playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
					if(ResumeAudioSources()) playableDirector.RebuildGraph(); // Resume all audio tracks
					playableDirector.time = cache_t;
					
					state = State.Playing;
				}
				else 
				{
					playableDirector.Play();
				}
			}
			else 
			{
				Restart();
			}
		}

		public void Play(PlayableAsset cutscene)
		{
			if (cutscene == null) return;
			
			this.cutscene = playableDirector.playableAsset = cutscene;

			Play();
		}

		public void Stop()
		{
			if (!stopped) playableDirector.Stop();
		}

		public void Cancel()
		{
			if (!playing) return;
			
			playableDirector.time = playableDirector.duration - .001f; // Move to the very end of cutscene
		}

		public void Restart()
		{
			playableDirector.time = 0.0;
			playableDirector.Play();
		}

		public void Pause()
		{
			if (playing) 
			{
				playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
				PauseAudioSources(); // Pause all audio tracks
				cache_t = (float)playableDirector.time;
				
				state = State.Paused;
			}
		}

		#endregion

		bool ResumeAudioSources()
		{
			var tracks = (playableDirector.playableAsset as TimelineAsset).GetOutputTracks();
			foreach (TrackAsset track in tracks) {
				if (track is AudioTrack) {
					track.muted = false; 
				}
			}

			return tracks.Count() > 0;
		}

		bool PauseAudioSources()
		{
			var tracks = (playableDirector.playableAsset as TimelineAsset).GetOutputTracks();Debug.Log(tracks.Count());
			foreach (TrackAsset track in tracks) {
				if (track is AudioTrack) {
					track.muted = true;
				}
			}
			
			return tracks.Count() > 0;
		}
	}
}