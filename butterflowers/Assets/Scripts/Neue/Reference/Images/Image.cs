using System;
using System.Collections.Generic;
using Neue.Reference.Nodes.Behaviours;
using UnityEngine;
using System.Linq;

namespace Neue.Reference.Images
{
	[Serializable]
	public class Image
	{
		List<IReadFrame> nodes = new List<IReadFrame>();
		public List<IReadFrame> Nodes => nodes;

		Camera _camera;
		public Camera Ref => _camera;

		public Image(IEnumerable<IReadFrame> nodes, Camera camera)
		{
			this.nodes = new List<IReadFrame>((nodes.OrderBy(node => Vector3.Distance(camera.transform.position, node.Object.transform.position)).ToArray()));
			_camera = camera;
		}

		public override string ToString()
		{
			string text = "";
			foreach (IReadFrame node in nodes) 
			{
				var vector = node.GetFrame();
				
				text += string.Format("node=> name: {0}  frame: {1}", node.Name, vector);
				text += "\n";
			}

			return text;
		}
	}
}