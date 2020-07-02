using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Log = Scribe.Log;

[System.Serializable] 
public class LogData 
{
    public string[] keys = new string[] { };
    public Log[] logs = new Log[] { };
}
