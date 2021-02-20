using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace uwu.Dialogue
{
	public class DialogueHandler : MonoBehaviour
	{
		#region Internal

		public enum SymbolType
		{
			Letter,
			Word
		}

		#endregion

		#region Collections

		[SerializeField] protected List<string> queue = new List<string>();

		#endregion

		#region Helpers

		string GetSymbol(ref int index, string body)
		{
			var ind = index;

			var current = body[ind];
			var start = ind;
			var end = ind;

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

			if (!includeSpaces)
				if (char.IsWhiteSpace(current)) {
					for (var i = ind; i < body.Length; i++)
						if (!char.IsWhiteSpace(body[i])) {
							end = i;
							break;
						}
						else {
							if (i == body.Length - 1)
								end = i;
						}

					ind = end;
					current = body[end];
				}

			if (symbol == SymbolType.Word)
				if (!char.IsWhiteSpace(current) || includeSpaces) {
					for (var i = ind; i < body.Length; i++) {
						var isspace = char.IsWhiteSpace(current);

						if (isspace != char.IsWhiteSpace(body[i])) {
							end = i - 1;
							break;
						}

						if (i == body.Length - 1)
							end = i;
					}

					current = body[end];
					ind = end;
				}

			var offset = end - start + 1;
			index += offset;

			return body.Substring(start, offset);
		}

		#endregion

		#region Events

		public Action onSpeak, onDispose;
		public Action<string> onProgress, onCompleteBody;

		#endregion

		#region External

		[SerializeField] UnityEngine.UI.Text txt_container = null;
		[SerializeField] TMP_Text tmp_container = null;

		#endregion

		#region Attributes

		[SerializeField] SymbolType symbol = SymbolType.Letter;
		[SerializeField] bool includeSpaces = true;

		[SerializeField] float m_timeBetweenSymbols = 1f;
		[SerializeField] float m_timeBetweenBodies = 1f;

		public bool autoprogress, autodispose;

		[SerializeField] bool speaking, waiting;

		[SerializeField] protected string m_body = "", m_current = "";

		#endregion

		#region Accessors

		[SerializeField] bool m_inprogress;

		public bool inprogress => queue.Count > 0;

		public bool available => queue.Count > 1;

		public bool reachedEndOfBody => current == body;

		public string body => m_body;

		public string current => m_current;

		public virtual float timeBetweenSymbols => m_timeBetweenSymbols;

		public virtual float timeBetweenBodies => m_timeBetweenBodies;

		public bool container => tmp_container != null || txt_container != null;

		#endregion

		#region Operations

		public virtual void Push(string body, bool immediate = false)
		{
			body = ParseBody(body);

			if (string.IsNullOrEmpty(body)) return;

			if (immediate) {
				if (inprogress) {
					if (available)
						queue.Insert(1, body);
					else
						queue.Add(body);

					Advance();
				}
				else {
					if (available)
						queue.Insert(0, body);
					else
						queue.Add(body);
				}
			}
			else {
				queue.Add(body);
			}


			if (!speaking) {
				OnSpeak();
				if (onSpeak != null)
					onSpeak();

				StartCoroutine("Speak");
			}
		}

		public void Push(string[] bodies)
		{
			foreach (var body in bodies)
				Push(body);
		}

		public virtual void Dispose()
		{
			OnDispose();
			if (onDispose != null)
				onDispose();
			
			if (speaking)
				StopCoroutine("Speak");

			queue = new List<string>();

			waiting = false;
			speaking = false;
		}

		public void Advance()
		{
			if (inprogress) {
				m_current = m_body;
				waiting = false;
			}
			else {
				Dispose();
			}
		}

		#endregion

		#region Internal

		protected virtual string ParseBody(string body)
		{
			return FilterBody(body);
		}

		protected virtual string FilterBody(string body)
		{
			return body;
		}

		protected virtual void SendBody(string body)
		{
			if (tmp_container != null) {
				tmp_container.text = body;
				return;
			}

			if (txt_container != null) txt_container.text = body;
		}

		IEnumerator Speak()
		{
			speaking = true;
			while (inprogress) {
				m_body = queue[0]; 
				m_current = "";

				OnStart(m_body);

				/*
			 * Parse existing body by symbol 
			 */

				var i = 0;
				while (!reachedEndOfBody) {
					m_current += GetSymbol(ref i, m_body);

					OnProgress();
					if (onProgress != null)
						onProgress(m_current);

					var t_symbol = timeBetweenSymbols;
					if (!reachedEndOfBody) yield return new WaitForSecondsRealtime(t_symbol);
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

				waiting = true;

				var t = 0f;
				var t_body = timeBetweenBodies;

				var pr_autoprogress = autoprogress;

				while (waiting && t < t_body) 
				{
					if (autoprogress) t += Time.unscaledDeltaTime;
					if (autoprogress != pr_autoprogress) // IMMEDIATELY BREAK FROM LOOP IF AUTOPROGRESS PARAM HAS CHANGED
						break;

					yield return null;
				}

				if (queue.Count > 0) queue.RemoveAt(0); // Pop current body
			}

			if (autodispose) Dispose();
		}

		#endregion

		#region Internal callbacks

		protected virtual void OnSpeak()
		{
		}

		protected virtual void OnStart(string body)
		{
		}

		protected virtual void OnProgress()
		{
			SendBody(current);
		}

		protected virtual void OnComplete(string body)
		{
		}

		protected virtual void OnDispose()
		{
			SendBody("");
		}

		#endregion
	}
}