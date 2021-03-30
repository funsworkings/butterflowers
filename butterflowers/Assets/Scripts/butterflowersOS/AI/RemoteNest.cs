using System;
using butterflowersOS.Objects.Entities.Interactables;
using UnityEngine;
using Random = UnityEngine.Random;

namespace butterflowersOS.AI
{
	public class RemoteNest : MonoBehaviour
	{
		// Properties

		Nest nest;
		
		// Attributes

		[SerializeField] float minKickStrength = 1f, maxKickStrength = 10f;


		void Awake()
		{
			nest = GetComponent<Nest>();
		}
		
		public void KickNest()
		{
			float strength = Random.Range(minKickStrength, maxKickStrength);
			nest.RandomKick(strength, events:false);
		}
	}
}