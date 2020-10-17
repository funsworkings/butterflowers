using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using uwu.Extensions;

[CreateAssetMenu(fileName = "New Dialogue Collection", menuName = "Extras/Dialogue Collection", order = 52)]
public class DialogueCollection : ScriptableObject
{
    [SerializeField] string prepend = "";
    [SerializeField] string append = "";

    public string[] elements;

    [SerializeField] List<string> visited = new List<string>();

    public string FetchRandomItem()
    {
        if (visited.Count == elements.Length)
            visited.Clear();

        string[] available = elements.Except(visited).ToArray();
        if (available.Length > 0) 
        {
            string el = available.PickRandomSubset(1)[0];

            if (!string.IsNullOrEmpty(el)) {
                visited.Add(el);

                return prepend + el + append;
            }
        }

        return null;
    }
}
