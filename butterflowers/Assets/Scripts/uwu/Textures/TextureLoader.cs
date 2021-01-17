using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using uwu.Extensions;
using uwu.IO;

namespace uwu.Textures
{
	public class TextureLoader : Singleton<TextureLoader>
	{
		// Collections
		
		[SerializeField] List<string> STACK = new List<string>();
		Dictionary<string, List<ITextureReceiver>> _receivers= new Dictionary<string, List<ITextureReceiver>>();

		// Properties

		private bool read = false;

		void Start()
		{
			StartCoroutine("LoadFromStack");
		}

		void OnDestroy()
		{
			StopAllCoroutines();
		}
		
		#region Load

		IEnumerator LoadFromStack()
		{
			while (true) 
			{
				if (STACK.Count > 0) 
				{
					if (!read) {
						var file = STACK[0];

						ReadBytes(file);
						read = true;
					}	
				}

				yield return null;
			}
		}
		
		async Task ReadBytes(string file)
		{
			var www = await new WWW(string.Format("file://{0}", file));
			
			Texture2D result = null;

			if (!string.IsNullOrEmpty(www.error))
				Debug.LogWarning("Error reading from texture => " + file);
			else 
			{
				Debug.LogWarning("Success load = " + file);
				
				result = www.texture;
				result.name = file;
			}
			
			Pop(file, result);
			read = false;
		}
		
		#endregion
		
		#region Stack

		public void Push(string file, ITextureReceiver receiver = null)
		{
			List<ITextureReceiver> receivers = new List<ITextureReceiver>();
			
			if (!STACK.Contains(file))
				STACK.Add(file);

			if (_receivers.ContainsKey(file)) 
			{
				if(!receivers.Contains(receiver))
					receivers.Add(receiver);
					
				_receivers[file] = receivers;
			}
			else {
				receivers.Add(receiver);
				_receivers.Add(file, receivers);
			}
		}

		void Pop(string file, Texture2D result)
		{
			List<ITextureReceiver> receivers = _receivers[file];
			foreach (ITextureReceiver receiver in receivers) 
			{
				receiver.ReceiveTexture(file, result);	
			}

			STACK.RemoveAt(0); // Remove first element in stack
			_receivers.Remove(file);
		}
		
		#endregion

	}
}