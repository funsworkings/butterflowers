using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;

/*
public class Timeline : MonoBehaviour
{
    Sun sun;
    FileNavigator navigator;

    public int elapsed = 0;
    public int days = 7;

    DateTime date;

    [SerializeField] string m_date_read = "";
    public string date_read
    {
        set
        {
            m_date_read = value;
            Debug.LogFormat("Arrived at date= {0}", m_date_read);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        sun = Sun.Instance;
        navigator = FileNavigator.Instance;

        Sun.onCycle += Advance;

        Init();
    }

    void Init()
    {
        date = new DateTime(2020, 8, 15); // Set default date
        UpdateDateTime();
    }

    void Advance()
    {
        if(++elapsed > days)
        {
            date = date.AddDays(-days);
            elapsed = 0;
        }
        else
            date = date.AddDays(1);

        UpdateDateTime();
    }

    void UpdateDateTime()
    {
        string shortdate = date.ToShortDateString();
        string[] date_comps = shortdate.Split('/');

        if (date_comps.Length == 3)
        {
            date_read = string.Format("{0}{1}{2}", date_comps[0], date_comps[1], date_comps[2]);
        }
    }

    void OnDestroy()
    {
        Sun.onCycle -= Advance;
    }
}*/
