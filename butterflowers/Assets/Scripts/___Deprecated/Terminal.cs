using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using uwu.Dialogue;

public class Terminal : DialogueHandler
{
    public UnityEvent onCompleteLogs;

    public bool complete = false;

    string logs = "";

    string last_logs = "";
    string current_log = "";


    protected override void OnStart(string body)
    {
        base.OnStart(body);

        last_logs = logs;
        current_log = body;
    }

    protected override void SendBody(string body)
    {
        string prepend = string.IsNullOrEmpty(last_logs) ? "" : "\n";
        string current = last_logs + prepend + body;
        base.SendBody(current);
    }

    protected override void OnComplete(string body)
    {
        base.OnComplete(body);

        if (!string.IsNullOrEmpty(logs))
            logs += string.Format("\n{0}", body);
        else
            logs += body;
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        onCompleteLogs.Invoke();
    }
}
