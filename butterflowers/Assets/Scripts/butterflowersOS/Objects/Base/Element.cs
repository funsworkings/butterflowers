using butterflowersOS.Core;
using butterflowersOS.Objects.Entities;
using UnityEngine;

namespace butterflowersOS.Objects.Base
{
	public class Element : MonoBehaviour
	{
		protected World World;
		protected Sun Sun;
		
		
		public bool Active => EvaluateActiveState();
		
		
		// Start is called before the first frame update
		protected void Start()
		{
			World = World.Instance;
			Sun = Sun.Instance;
        
			World.RegisterEntity(this);
        
			OnStart();
		}

		protected virtual void Update()
		{
			if(World.LOAD && EvaluateActiveState())
				OnUpdate();
		}

		bool quit = false;
		protected void OnApplicationQuit()
		{
			quit = true;
		}

		protected void OnDestroy()
		{
			if (quit) 
			{
				Debug.LogWarning("Application quitting, ignore request to destroy el!");
				return;
			}

			if(World != null) World.UnregisterEntity(this);
			OnDestroyed();
		}

		#region Monobehaviour override callbacks

		protected virtual void OnStart() { }
		protected virtual void OnUpdate() { }
		protected virtual void OnDestroyed() { }

		#endregion
		
		#region Verify update

		protected virtual bool EvaluateActiveState()
		{
			return true;
		}

		#endregion
	}
}