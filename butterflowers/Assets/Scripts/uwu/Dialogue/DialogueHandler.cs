using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UIExt.Behaviors.Visibility;
using UnityEngine.Experimental.UIElements;

public class DialogueHandler : MonoBehaviour {

    #region Events

    public System.Action onSpeak, onDispose;
    public System.Action<string> onProgress, onCompleteBody;

    #endregion

    #region External

	[SerializeField] TMP_Text container = null;

    #endregion

    #region Internal

    public enum SymbolType { Letter, Word }

	#endregion

	#region Collections

	List<string> queue = new List<string>();

    #endregion

    #region Attributes

    [SerializeField] SymbolType symbol = SymbolType.Letter;
    [SerializeField] bool includeSpaces = true;

    [SerializeField] float timeBetweenSymbols = 1f;
    [SerializeField] float timeBetweenBodies = 1f;
    [SerializeField] bool autoprogress = false, autodispose = false;

    [SerializeField] bool speaking = false, waiting = false;

    [SerializeField] protected string m_body = "", m_current = "";

    #endregion

    #region Accessors

    [SerializeField] bool m_inprogress = false;
	public bool inprogress {
        get
        {
            return queue.Count > 0;
        }
    }

    public bool reachedEndOfBody {
        get
        {
            return current == body;
        }
    }

    public string body {
        get
        {
            return m_body;
        }
    }

    public string current {
        get
        {
            return m_current;
        }
    }

	#endregion

	#region Operations

	public virtual void Push(string body)
    {
        Debug.Log("receive = " + body);
        body = ParseBody(body);

        if (string.IsNullOrEmpty(body)) return;
        if (container == null) return;

        queue.Add(body);

        if (!speaking) 
        {
            OnSpeak();
            if (onSpeak != null)
                onSpeak();

            StartCoroutine("Speak");
        }
    }

    public virtual void Dispose()
    {
        if (speaking) {
            OnDispose();
            if (onDispose != null)
                onDispose();

            StopCoroutine("Speak");
        }

        queue = new List<string>();

        waiting = false;
        speaking = false;
    }

    public void Advance()
    {
        if (inprogress) 
        {
            m_current = m_body;
            waiting = false;
        }
        else 
        {
            Dispose();
        }
    }

    #endregion

    #region Internal

    protected virtual string ParseBody(string body) { return FilterBody(body); }
    protected virtual string FilterBody(string body) { return body; }

	IEnumerator Speak()
    {
        speaking = true;
        while (inprogress) 
        {
            m_body = queue[0];
            m_current = "";

            OnStart(m_body);

            /*
             * Parse existing body by symbol 
             */

            int i = 0;
            while(!reachedEndOfBody) {
                m_current += GetSymbol(ref i, m_body);

                OnProgress();
                if (onProgress != null)
                    onProgress(m_current);

                if (!reachedEndOfBody) yield return new WaitForSeconds(timeBetweenSymbols);
            }

            m_current = m_body;
            if (i < body.Length) { // Premature cancel
                OnProgress();
                if (onProgress != null)
                    onProgress(m_current);
            }

            OnComplete(current);
            if (onCompleteBody != null) // Fire event for reached end of body
                onCompleteBody(current);

            waiting = true;  var t = 0f;
            while (waiting && t < timeBetweenBodies) {
                if (autoprogress) 
                {
                    t += Time.deltaTime;
                }
                yield return null;
            }

            if (queue.Count > 0) queue.RemoveAt(0); // Pop current body
        }
        
        if(autodispose) Dispose();
    }

    #endregion

    #region Internal callbacks

    protected virtual void OnSpeak() { }
    protected virtual void OnStart(string body) { }
    protected virtual void OnProgress() { container.text = current; }
    protected virtual void OnComplete(string body) { }
    protected virtual void OnDispose() { container.text = ""; }

    #endregion

    #region Helpers

    string GetSymbol(ref int index, string body)
    {
        int ind = index;
        char current = body[ind];

        int start = ind;
        int end = ind;

        // Detected rich text
        /*if (current == '<') {
            for (int i = index; i < body.Length; i++) {
                if (body[i] == '>') {
                    end = i;
                    break;
                }
            }
            current = body[end];
        }*/

        if (!includeSpaces) {
            if (char.IsWhiteSpace(current)) {
                for (int i = ind; i < body.Length; i++) {
                    if (!char.IsWhiteSpace(body[i])) {
                        end = i - 1;
                        break;
                    }
                    else {
                        if (i == body.Length - 1)
                            end = i;
                    }
                }
                current = body[end];
                ind = end;
            }
        }

        if (symbol == SymbolType.Word) 
        {
            if (!char.IsWhiteSpace(current) || includeSpaces) {
                for (int i = ind; i < body.Length; i++) {
                    var isspace = char.IsWhiteSpace(current);

                    if (isspace != char.IsWhiteSpace(body[i])) {
                        end = i - 1;
                        break;
                    }
                    else {
                        if (i == body.Length - 1) 
                            end = i;
                    }
                }
                current = body[end];
                ind = end;
            }
        }
        
        var offset = (end - start) + 1;
        index += offset;
           
        return body.Substring(start, offset);
    }

	#endregion
}
