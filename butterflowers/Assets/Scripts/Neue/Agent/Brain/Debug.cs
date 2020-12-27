using System;
using System.Collections.Generic;
using System.Linq;
using Neue.Reference.Images;
using Neue.Reference.Nodes.Behaviours;
using Neue.Reference.Nodes.Debug;
using UnityEngine;
using uwu.UI.Behaviors.Visibility;

namespace Neue.Agent.Brain
{
	public class Debug : MonoBehaviour
	{
		// Properties

		Capture _capture;
		
		[SerializeField] GameObject debugPrefab;
		[SerializeField] Transform debugContainer;
		[SerializeField] ToggleOpacity debugOpacity;
		
		// Attributes

		[Header("Toggles")]
		[SerializeField] bool showFrames = false;
		
		// Collections
		
		Dictionary<INode, NodeFrame> debugNodes = new Dictionary<INode, NodeFrame>();


		void Awake()
		{
			_capture = GetComponent<Capture>();
		}

		void OnEnable()
		{
			_capture.onCreateImage += onCreatedImage;
		}

		void OnDisable()
		{
			_capture.onCreateImage -= onCreatedImage;
		}

		void Update()
		{
			if (showFrames) 
			{
				UpdateNodes();
				debugOpacity.Show();
			}
			else debugOpacity.Hide();
		}

		void OnDestroy()
		{
			ClearDebugNodes();
		}

		void OnDrawGizmos()
		{
			var _collider = _capture.Collider;
			if (_collider == null) return;
			
			var collider_t = _collider.transform;
			
			Gizmos.color = Color.white;
			Gizmos.matrix = Matrix4x4.TRS(collider_t.position, collider_t.rotation, collider_t.lossyScale);

			Gizmos.DrawWireCube(_collider.center, _collider.size);
		}
		
		
		#region Nodes

		void onCreatedImage(Image image)
		{
			ClearDebugNodes();
			if (image == null) return;
			
			var nodes = image.Nodes;
			foreach (IReadFrame node in nodes) 
				AddDebugNode(node, image.Ref);
		}
		
		void AddDebugNode(IReadFrame node, Camera camera)
		{
			var instance = Instantiate(debugPrefab, debugContainer);
			var nodeFrame = instance.GetComponent<NodeFrame>();
			nodeFrame.SetFromNode(node, camera);

			debugNodes.Add(node, nodeFrame);
		}

		void ClearDebugNodes()
		{
			NodeFrame[] debugNodes = this.debugNodes.Select(dn => dn.Value).ToArray();
			foreach(NodeFrame debugNode in debugNodes)
				Destroy(debugNode.gameObject);
			
			this.debugNodes = new Dictionary<INode, NodeFrame>();
		}

		void UpdateNodes()
		{
			foreach (KeyValuePair<INode, NodeFrame> debugNode in debugNodes) 
			{
				var node = debugNode.Key;
				var debug = debugNode.Value;
				
				if(node is IReadFrame) debug.SetFromNode(node as IReadFrame);
			}
		}
		
		#endregion
	}
}