using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;

namespace butterflowersOS.Objects.Entities.Interactables.Empty
{
	public class Star : Focusable, ITooltip
	{
		public string GetInfo()
		{
			return "magic star".AppendActionableInformation(this);
		}
	}
}