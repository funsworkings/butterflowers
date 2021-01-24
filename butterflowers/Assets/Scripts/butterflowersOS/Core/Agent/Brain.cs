using butterflowersOS.Core.Agent.Modules;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Managers;
using UnityEngine;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Core.Agent
{
	public class Brain : MonoBehaviour
	{
		// External

		[SerializeField] Sun Sun;
		[SerializeField] World World;
		[SerializeField] Nest Nest;

		[SerializeField] Surveillance Surveillance;
		[SerializeField] BeaconManager Beacons;
		[SerializeField] VineManager Vines;
		
		// Properties

		FOV fov;

		[SerializeField] ToggleOpacity uiPanel;


		void Awake()
		{
			fov = GetComponent<FOV>();
		}
		
		void OnEnable()
		{
			uiPanel.Show();
		}

		void OnDisable()
		{
			uiPanel.Hide();
		}
		
		
		#region Visibility
		
		
		
		#endregion
	}
}