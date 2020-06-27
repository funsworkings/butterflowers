using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unknown : MonoBehaviour
{

    #region External

    [SerializeField] MotherOfButterflies Butterflies;

	#endregion

	#region Attributes

	[SerializeField] float scribeInterval = 1f;
    [SerializeField] int river = 0;

    [SerializeField] int riverwidth = 8, minriverwidth = 2, maxriverwidth = 16;
    [SerializeField] int totalwidth = 32;

    [SerializeField] float minriverwavelength = 0f, maxriverwavelength = 3.33f;
    [SerializeField] float riverspeed = 1f;

    [SerializeField] char riverchar = '~';
    [SerializeField] char normalchar = '0';

    #endregion

    bool plague = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) {
            plague = !plague;

            if (plague) StartCoroutine("Plague");
            else StopCoroutine("Plague");
        }
    }

    IEnumerator Plague()
    {
        while (true) 
        {
            updateRiver();

            var @string = fetchRiver();
            Scribe.Instance.Push(EVENTCODE.UNKNOWN, AGENT.Unknown, AGENT.World, @string);

            yield return new WaitForSeconds(Mathf.Max(0f, scribeInterval));
        }
    }

    void updateRiver()
    {
        float health = Butterflies.GetHealth();

        float wavelength = health.RemapNRB(0f, 1f, minriverwavelength, maxriverwavelength);
        float slope = -(wavelength * Mathf.Sin(Time.time * riverspeed));

        river += Mathf.RoundToInt(slope);

        float size = Random.Range(-1f, 1f);
        riverwidth = Mathf.Clamp(riverwidth + Mathf.RoundToInt(size), minriverwidth, maxriverwidth);
    }

    string fetchRiver()
    {
        var row = new string(normalchar, totalwidth);

        int start = Mathf.Clamp(river - riverwidth/2, 0, totalwidth-1);
        int end = Mathf.Clamp(river + riverwidth / 2, 0, totalwidth-1);

        if (start == end) return row;

        var chars = row.ToCharArray();
        for (int i = start; i <= end; i++)
            chars[i] = riverchar;

        return new string(chars);
    }
}
