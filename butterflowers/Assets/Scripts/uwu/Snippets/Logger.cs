using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Logger : MonoBehaviour
{
	#region Collections

	protected List<string> messages = new List<string>();

	#endregion

	#region Attributes

	[SerializeField] int capacity = 10;
    [SerializeField] protected bool duplicates = true;
    [SerializeField] protected bool auto = false, resize = false;

	#endregion

	public void Push(string message)
    {
        var duplicate = messages.Contains(message);
        if (!duplicates)
            return;

        if (resize) capacity = messages.Count+1;

        var overflow = (messages.Count + 1) > capacity;
        if (overflow) {
            if (auto) Pop();
            else {
                Debug.LogWarning("Attempt to add element to exceed capacity of fixed logger, ignore..");
                return;
            }
        }

        messages.Add(message);
        onPushElement(message);
    }

    void Pop()
    {
        if (messages.Count > 0)
            Remove(0);
    }

    public void Remove(int index)
    {
        if (index >= 0 && index <= messages.Count - 1) {
            var message = messages[index];
            messages.RemoveAt(index);
            onPopElement(message);
        }
    }

    public void Clear()
    {
        var emptying = messages.ToArray();
        messages = new List<string>();

        for (int i = 0; i < emptying.Length; i++)
            onPopElement(emptying[i]);
    }

    #region Callbacks

    protected virtual void onPushElement(string message) { }
    protected virtual void onPopElement(string message) { }

	#endregion
}
