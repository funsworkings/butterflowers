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
	public class TextureLoader : MonoBehaviour
	{
		// Collections
		
		[SerializeField] List<string> STACK = new List<string>();
		Dictionary<string, List<ITextureReceiver>> _receivers= new Dictionary<string, List<ITextureReceiver>>();

		void Start()
		{
			//Texture.allowThreadedTextureCreation = true;
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
					//if (!read) 
					//{
						var file = STACK[0];
						Debug.LogWarning($"Try load {file}");

						
#pragma warning disable 618
						var www = new WWW(string.Format("file://{0}", file));
						yield return www;
#pragma warning restore 618
			
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
					//	read = false;
			
						www.Dispose(); // Dispose WWW!
						
						
					//	read = true;
					//}	
				}

				yield return null;
			}
		}
		
		async Task ReadBytes(string file)
		{
#pragma warning disable 618
			var www = await new WWW(string.Format("file://{0}", file));
#pragma warning restore 618
			
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

			www.Dispose(); // Dispose WWW!
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