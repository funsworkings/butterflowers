using System;
using System.Collections.Generic;
using System.Linq;
using Neue.Agent.Types;
using Neue.Reference.Images;
using Neue.Reference.Nodes.Behaviours;
using Neue.Reference.Nodes.Debug;
using UnityEngine;

namespace Neue.Agent.Brain
{
	public class Capture : Module
	{
		// Events

		public System.Action<Image> onCreateImage;
		
		// Properties

		new Camera camera;
		new BoxCollider collider;
		
		Image _image;

		// Attributes

		[SerializeField] float maxCaptureDistance = 1f;
		[SerializeField] bool debug = false;
		
		// Collections
		
		List<NodeFrame> debugNodes = new List<NodeFrame>();
		
		#region Accessors

		public BoxCollider Collider => collider;
		public Camera Camera => camera;

		public Image Image => _image;
		
		#endregion

		void Start()
		{
			camera = Camera.main;
			collider = CreateCollider();
		}

		#region Ops

		public void CreateImage()
		{
			UpdateColliderSize(maxCaptureDistance);

			var collider_t = collider.transform;
			
			var center = collider_t.TransformPoint(collider.center);
			var extents = collider.size/2f;
				extents.Scale(collider_t.lossyScale);
			var rotation = collider_t.rotation;

			var collisions = Physics.OverlapBox(center, extents, rotation);
			var readables = new List<IReadFrame>();

			foreach (Collider collider in collisions) 
			{
				var readable = collider.GetComponents<MonoBehaviour>().OfType<IReadFrame>();
				if (readable.Count() > 0)
					readables.AddRange(readable);
			}
			
			_image = new Image(readables, camera); // Construct image

			if (onCreateImage != null)
				onCreateImage(_image);
			
			print(_image);
		}

		public void WipeImage()
		{
			_image = null;
		}
		
		#endregion
		
		#region Bounds

		BoxCollider CreateCollider()
		{
			var instance = new GameObject("Capture (collider)");
				instance.transform.parent = camera.transform;
				instance.layer = LayerMask.NameToLayer("Ignore Raycast");
				
			var _collider = instance.AddComponent<BoxCollider>();
			_collider.isTrigger = true;

			return _collider;
		}

		void UpdateColliderSize(float thickness)
		{
			var ncp = camera.nearClipPlane;
			var fov = camera.fieldOfView;
			var asp = camera.aspect;

			var depth = ncp + (0.01f + thickness / 2f);

			var transform = collider.transform;
			
				transform.localEulerAngles = Vector3.zero;
				transform.localPosition = new Vector3(0f, 0f, depth);

			var height = Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f) * depth * 2f;
				transform.localScale = new Vector3(height * asp, height, thickness);
		}
		
		#endregion

		#region Module

		public override void Continue()
		{
			//if(Input.GetKeyDown(KeyCode.C)) CreateImage();
		}

		public override void Pause()
		{
			throw new NotImplementedException();
		}

		public override void Destroy()
		{
			throw new NotImplementedException();
		}
		
		#endregion
	}
}