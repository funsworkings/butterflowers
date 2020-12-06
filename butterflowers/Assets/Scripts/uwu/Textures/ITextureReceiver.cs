using UnityEngine;

namespace uwu.Textures
{
	public interface ITextureReceiver
	{
		void ReceiveTexture(string file, Texture2D texture);
	}
}