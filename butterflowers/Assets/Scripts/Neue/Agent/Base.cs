using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neue.Agent.Actions.Movement;
using Neue.Agent.Brain;
using Neue.Agent.Types;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Neue.Agent
{
	public class Base : MonoBehaviour
	{
		// Properties

		[SerializeField] Module[] modules;

		// Collections
		
		Dictionary<Type, Module> moduleTypeLookup = new Dictionary<Type,Module>();
		
		#region Accessors

		FieldOfView FOV => GetModule<FieldOfView>();
		Capture Capture => GetModule<Capture>();
		Navigation Navigation => GetModule<Navigation>();
		Priorities Priorities => GetModule<Priorities>();
		
		#endregion
		
		IEnumerator Start()
		{
			modules = GetComponents<MonoBehaviour>().OfType<Module>().ToArray();
			yield return new WaitForEndOfFrame();

			foreach (Module module in modules) 
			{
				module.Init(this);
				
				var type = module.GetType();
				if (!moduleTypeLookup.ContainsKey(type))
					moduleTypeLookup.Add(type, module);
				else {
					moduleTypeLookup[type] = module;
					Debug.LogWarningFormat("Base was forced to overwrite module for type \"{0}\"", type.FullName);
				}
			}
		}

		void Update()
		{
			foreach(Module module in modules) module.Continue();
		}

		void OnDestroy()
		{
			foreach(Module module in modules) module.Destroy();
		}
		
		
		#region Lookup

		public E GetModule<E>() where E : Module
		{
			var type = typeof(E);
			if (moduleTypeLookup.ContainsKey(type))
				return (E)moduleTypeLookup[type];

			return null;
		}
		
		#endregion
	}
}