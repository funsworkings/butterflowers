using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class Scribe : Logger
{
	#region Collections

	public Dictionary<string, TMP_Text> logs = new Dictionary<string, TMP_Text>();

	#endregion

	#region Attributes

	[SerializeField] GameObject messagePrefab = null;

	#endregion

	#region Monobehaviour callbacks

	void Awake()
	{
		duplicates = true;
		//auto = true;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.S)) {
			string msg = Extensions.RandomString(Random.Range(16, 64));
			Push(msg);
		}
	}

	void OnDisable()
	{
		Clear();
	}

	#endregion

	TMP_Text InstantiateLog(string message)
	{
		var msg = Instantiate(messagePrefab, transform);
		var tmp = msg.GetComponent<TMP_Text>();
			tmp.text = message;

		return tmp;
	}

	#region Logger overrides

	protected override void onPopElement(string message)
	{
		var tmp_text = logs[message];
		if (tmp_text != null) {
			GameObject.Destroy(tmp_text.gameObject);
			logs.Remove(message);
		}
	}

	protected override void onPushElement(string message)
	{
		var tmp_text = InstantiateLog(message);
		logs.Add(message, tmp_text);
	}

	#endregion
}
