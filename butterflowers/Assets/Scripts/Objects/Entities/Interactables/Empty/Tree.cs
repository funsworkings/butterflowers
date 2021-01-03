using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Objects.Base;
using UnityEngine;

public class Tree : Focusable, ITooltip
{
	public string GetInfo()
	{
		return "treeeeeee".AppendActionableInformation(this);
	}
}
