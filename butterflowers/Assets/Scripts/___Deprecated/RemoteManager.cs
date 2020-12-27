using System;
using Objects.Types;
using Settings;
using UnityEngine;
using uwu;
using uwu.UI.Behaviors.Visibility;

namespace Objects.Managers
{
	[Obsolete("Obsolete API!", true)]
	public class RemoteManager : MonoBehaviour, ISaveable
	{
		// Events

		public System.Action<WorldState> onUpdateState;
		public System.Action onParallel;
		
		// External

		[SerializeField] Wand wand;
		
		// Properties

		bool load = false;

		[SerializeField] WorldPreset preset;
		[SerializeField] Cage cage;
		[SerializeField] WorldState state = WorldState.User;
		
		[SerializeField] UnityEngine.UI.Image remoteProgressBar;
		[SerializeField] ToggleOpacity userOverlay, remoteOverlay;
		
		// Attributes

		[SerializeField] float remoteTimer = 0f;
		
		#region Accessors

		public WorldState _State
		{
			get { return state; }
			set { state = value;  }
		}
		
		#endregion

		void OnEnable()
		{
			cage.onCompleteCorners += TriggerParallel;
		}

		void OnDisable()
		{
			cage.onCompleteCorners -= TriggerParallel;
		}

		void Update()
		{
			if (!load) return;
			
			WorldState _state = UpdateRemoteStatus();
			if (_state != state) 
			{
				state = _state;

				if (onUpdateState != null)
					onUpdateState(state);
			}
		}
		
		
		#region Remote trigger
		
		WorldState UpdateRemoteStatus()
            {
                WorldState targetState = WorldState.User;
        
                if (state == WorldState.Parallel) 
                {
                    remoteTimer = 0f;
                    targetState = WorldState.Parallel;
                    UpdateUserInterfaces(-1f);
                }
                else 
                {
                    if (!wand.spells || wand.speed2d < preset.cursorSpeedThreshold) 
                    {
                        remoteTimer += Time.deltaTime;
        
                        float remoteInterval =
                            Mathf.Clamp01((remoteTimer - preset.cursorIdleDelay) / preset.cursorIdleTimeThreshold);
                        targetState = (remoteInterval >= 1f) ? WorldState.Remote : WorldState.User;
        
                        UpdateUserInterfaces(remoteInterval);
                    }
                    else {
                        remoteTimer = 0f;
                        targetState = WorldState.User;
        
                        UpdateUserInterfaces(0f);
                    }
                }
        
                return targetState;
            }

		void TriggerParallel()
            {
                bool @new = (state != WorldState.Parallel);
        
                if (@new) 
                {
	                if (onParallel != null)
	                    onParallel();
                }
                
                state = WorldState.Parallel; // Initialize parallel AI
            }
		
		#endregion
		
		#region UI 
		
		void UpdateUserInterfaces(float progress)
		{
			bool user = (state == WorldState.User);
	            
			if (!user) 
			{
				remoteOverlay.Show();
        
				if (progress < 0f)
					userOverlay.Show();
				else
					userOverlay.Hide();
			}
			else {
				remoteOverlay.Hide();
				userOverlay.Show();
			}
        
			remoteProgressBar.fillAmount = (!user) ? 0f : progress;
		}
		
		#endregion
		
		#region Save/load
		
		public object Save()
		{
			return state;
		}

		public void Load(object data)
		{
			var t_state = (WorldState) data;
			state = (t_state == WorldState.Parallel) ? WorldState.Parallel : WorldState.User;
			load = true;
		}
		
		#endregion
	}
}