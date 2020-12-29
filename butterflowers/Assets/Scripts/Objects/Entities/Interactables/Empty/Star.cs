using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

public class Star : Focusable, ITooltip
{
	public string GetInfo()
	{
		return "magic star";
	}
}