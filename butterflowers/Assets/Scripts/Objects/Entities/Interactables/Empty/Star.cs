using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Objects.Base;
using UnityEngine;

public class Star : Focusable, ITooltip
{
	public string GetInfo()
	{
		return "magic star".AppendActionableInformation(this);
	}
}