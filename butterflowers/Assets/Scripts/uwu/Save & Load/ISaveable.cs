using System;

namespace uwu
{
	public interface ISaveable
	{
		System.Object Save();
		void Load(System.Object data);
	}
}