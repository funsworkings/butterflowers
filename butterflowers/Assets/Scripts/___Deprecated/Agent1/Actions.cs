using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS;
using butterflowersOS.Core;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Managers;
using Neue.Agent.Actions.Types;
using Neue.Reference.Types.Maps.Groups;
using Neue.Types;
using Noder.Graphs;
using UnityEngine;
using uwu.Camera;
using uwu.Extensions;
using uwu.Snippets;
using Action = Neue.Types.Action;
using Random = UnityEngine.Random;

namespace Neue.Agent1
{
	[Obsolete("Obsolete agent!", true)]
	public class Actions : MonoBehaviour
	{
		// Events

		public System.Action<Action> onEnactAction, onCompleteAction, onFailAction;

		// External

		CameraManager CameraManager;

		[SerializeField] Nest Nest;
		[SerializeField] BeaconManager Beacons;
		[SerializeField] Focusing Focus;
		[SerializeField] CameraVisualBlend CameraBlend;

		// Properties

		[SerializeField] Agent.Presets.BrainPreset Preset;
		[SerializeField] Wand wand;
		[SerializeField] GameObject avatar;

		Brain Brain;
		Body _body;
		Damage damage;
		new Camera camera;

		// Collections

		[SerializeField] List<Action> m_queue = new List<Action>();

		// Attributes

		[SerializeField] bool m_inprogress = false;
		[SerializeField] Action m_currentAction = null;
		[SerializeField] float defaultKickForce = 1f;

		#region Accessors

		public Action currentAction => m_currentAction;
		public Action[] queue => m_queue.ToArray();

		public Action nextAction
		{
			get
			{
				if (available) return queue[0];

				return null;
			}
		}

		public bool inprogress
		{
			get => m_inprogress;
			set => m_inprogress = value;
		}

		public bool available => m_queue != null && m_queue.Count > 0;

		public float radius => Preset.actionRadius;

		public Body body => _body;

		#endregion

		#region Monobehaviour callbacks

		void Awake()
		{
			Brain = GetComponent<Brain>();

			avatar.SetActive(false);

			_body = avatar.GetComponentInChildren<Body>(true);
			if (_body != null) damage = avatar.GetComponent<Damage>();
		}

		void Start()
		{
			CameraManager = FindObjectOfType<CameraManager>();
		}

		void Update()
		{
			avatar.SetActive(Brain.Self);
		}

		void OnEnable()
		{
			_body.DidTeleport += OnDidTeleport;
			_body.DidCastSpell += OnDidCastSpell;

			StartCoroutine("Refresh");
		}

		void OnDisable()
		{
			_body.DidTeleport -= OnDidTeleport;
			_body.DidCastSpell -= OnDidCastSpell;

			StopCoroutine("Refresh");
		}

		#endregion


		IEnumerator Refresh()
		{
			while (true) {
				var action = nextAction;

				if (available && action != null) {
					if (!inprogress) {
						var next = queue[0];
						var delay = next.delay;

						yield return new WaitForSeconds(delay);
					}

					Pop(action);
				}

				yield return null;
			}
		}


		#region Action queue

		public void Push(Action action)
		{
			Push(action.@event, action.dat, action.root, action.immediate, rewards: action.rewards);
		}

		public void Push(ActionSequence actions)
		{
			var seq = actions.actions;
			var immediate = actions.immediate;

			for (var i = 0; i < seq.Length; i++) {
				var action = seq[i];
				action.immediate = false;

				if (immediate)
					m_queue.Insert(i, action);
				else
					m_queue.Add(action);
			}
		}

		public void Push(EVENTCODE @event, object dat = null, Transform root = null, bool immediate = false,
			float delay = -1f, bool auto = false, FrameIntGroup rewards = null, ModuleTree tree = null)
		{
			var action = new Action();
			action.@event = @event;
			action.dat = dat;
			action.root = root == null ? ParseRoot(@event, dat) : root;
			action.tree = tree;
			action.rewards = rewards;
			action.immediate = immediate;
			action.delay = delay < 0f
				? Random.Range(Preset.minimumTimeBetweenActions, Preset.maximumTimeBetweenActions)
				: delay;
			action.auto = auto;

			if (immediate) {
				if (available)
					m_queue.Insert(0, action);
				else
					m_queue.Add(action);

				if (currentAction.@event != EVENTCODE.NULL && inprogress) {
					var cancel = CancelAction(currentAction);
					inprogress = !cancel;
				}

				if (action.delay == 0f) Pop();
			}
			else {
				m_queue.Add(action);
			}
		}

		void Pop(Action action = null)
		{
			if (!available) return;
			inprogress = true;

			if (action != null) {
				if (action != currentAction) {
					try {
						ValidateAction(action);

						m_currentAction = action;
						body.cast = body.spell = false; // Reset body spellcast

						Pop(action);
					}
					catch (System.Exception e) {
						var failureCode = e.Data["fail"];
						Debug.LogWarningFormat("Failed to validate action => {0}", failureCode);

						switch (failureCode) {
							case FailureCode.MaximumDistance:

								if (!body.teleport)
									body.TeleportToLocation(action
										.root); // Move body to location until maximum distance reached

								break;
							case FailureCode.NotVisible:

								if (CameraBlend.Blending) {
									Debug.LogWarning("Camera visual blend in progress, wait until complete to move!");
								}
								else {
									var focuses = Focus.FindVisibleFocuses();

									if (focuses.Length > 0) {
										var sortedFocuses =
											Brain.SortFocusesByAlignmentToTarget(focuses, action.root);
										var range = Mathf.FloorToInt((1f - Brain.stance) * sortedFocuses.Length);

										var focus = range == 0
											? sortedFocuses[0]
											: sortedFocuses[Random.Range(0, range)];
										if (focus == null)
											Focus.LoseFocus();
										else
											focus.Focus(); // Focus on point
									}
									else {
										Dispose(); // Wipe action if no available way to get there!
									}
								}

								break;
							case FailureCode.Uncast:

								if (!body.spell)
									body.Spell();

								break;
							default:
								Dispose(); // Wipe all actions if NULL
								Debug.LogWarning("Dispose !!");
								break;
						}
					}
				}
				else {
					try {
						ParseAction(action);
					}
					catch (System.Exception err) {
						Debug.LogWarningFormat("Failed to parse action => {0}", err.Data["fail"]);

						if (onFailAction != null)
							onFailAction(action);
					}

					m_queue.RemoveAt(0); // Remove el from queue
					inprogress = false;
				}
			}
		}

		public void Dispose()
		{
			m_queue.Clear();

			if (inprogress) {
				var clear = CancelAction(currentAction);
				if (clear) {
					m_currentAction = null;
					inprogress = false;
				}
			}

			body.spell = body.cast = false;
		}

		#endregion

		#region Action parse + cancel

		Transform ParseRoot(EVENTCODE @event, object dat)
		{
			Transform root = null;
			var eventcode = System.Enum.GetName(typeof(EVENTCODE), @event);

			if (eventcode.Contains("NEST")) {
				root = Nest.transform;
			}
			else if (eventcode.Contains("BEACON")) {
				var beacon = (Beacon) dat;
				if (beacon != null)
					root = beacon.transform;
			}
			else if (@event == EVENTCODE.REFOCUS) {
				if (dat != null)
					root = ((Focusable) dat).transform;
			}

			return root;
		}

		void ValidateAction(Action action)
		{
			var exception = new System.Exception();

			if (!Brain.isActive || !Sun.Instance.active) {
				exception.Data["fail"] = FailureCode.Inactive;
				throw exception;
			}

			var root = action.root;
			if (root == null) {
				exception.Data["fail"] = FailureCode.MissingObject;
				throw exception;
			}

			if (Brain.Self) {
				var distance = Vector3.Distance(root.position, body.transform.position);

				if (!body.spell)
					if (distance > radius) {
						exception.Data["fail"] = FailureCode.MaximumDistance;
						throw exception;
					}


				if (!body.cast) {
					exception.Data["fail"] = FailureCode.Uncast;
					throw exception;
				}

				if (distance > radius) {
					exception.Data["fail"] =
						FailureCode.MissingObject; // Object was moved while in the middle of cast, FOILED!
					throw exception;
				}
			}
			else if (Brain.Remote) {
				var screen = Vector2.zero;
				var camera = CameraManager.MainCamera;

				var visible = root.IsVisible(camera, out screen);
				if (!visible) {
					exception.Data["fail"] = FailureCode.NotVisible;
					throw exception;
				}
			}
		}

		void ParseAction(Action action)
		{
			var dat = action.dat;
			var root = action.root;
			var @event = action.@event;
			var t = System.Enum.GetName(typeof(EVENTCODE), @event);
			var rewards = action.rewards;

			var success = root != null;
			//success = false; // Override missing object

			if (success) { // Passed the action radius check

				if (t.StartsWith("BEACON")) {
					var beacon = (Beacon) dat;

					if (beacon != null) {
						if (@event == EVENTCODE.BEACONACTIVATE)
							success = wand.ActivateBeacon(beacon);
						else if (@event == EVENTCODE.BEACONDELETE)
							success = wand.DestroyBeacon(beacon);
						else if (@event == EVENTCODE.BEACONPLANT)
							success = wand.PlantBeacon(beacon);
					}
				}
				else if (t.StartsWith("NEST")) {
					if (@event == EVENTCODE.NESTPOP) {
						var beacon = (Beacon) dat;

						if (beacon == null)
							success = wand.PopLastBeaconFromNest();
						else
							success = wand.PopBeaconFromNest(beacon);
					}
					else if (@event == EVENTCODE.NESTKICK) {
						var kick = new Wand.Kick();
						if (dat == null) {
							kick.useDirection = false;
							kick.force = defaultKickForce;

							print("DEF KCIK");
						}
						else {
							kick = (Wand.Kick) dat; // Update kick from data
						}

						success = wand.KickNest(kick);
					}
					else if (@event == EVENTCODE.NESTCLEAR) {
						success = wand.ClearNest();
					}
				}
				else // Miscellaneous events
				{
					if (@event == EVENTCODE.REFOCUS) {
						if (dat == null) {
							if (!Brain.Self)
								success = wand.EscapeFocus();
							else
								success = body.Detach();
						}
						else {
							var focus = (Focusable) dat;

							//if (!Brain.Self)
							success = wand.Refocus(focus);
							//else
							//    success = body.Attach(focus);
						}
					}
				}
			}

			if (success) {
				if (onCompleteAction != null)
					onCompleteAction(action);

				action.Complete();
			}
			else {
				var e = new System.Exception();
				e.Data["fail"] = FailureCode.MissingObject;

				throw e;
			}
		}

		bool CancelAction(Action action)
		{
			if (action != null) action.Cancel();

			return true;
		}

		#endregion

		#region Mesh animation callbacks

		void OnDidCastSpell()
		{
			//Pop(currentAction);
		}

		void OnDidTeleport()
		{
		}

		#endregion

		#region Miscellaneous

		public void Hit()
		{
			damage.Hit();
		}

		#endregion
	}
}