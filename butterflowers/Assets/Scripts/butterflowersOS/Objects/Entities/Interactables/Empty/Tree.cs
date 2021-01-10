using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;

namespace butterflowersOS.Objects.Entities.Interactables.Empty
{
	public class Tree : Focusable, ITooltip
	{
		public string GetInfo()
		{
			return "treeeeeee".AppendActionableInformation(this);
		}
	}
}
