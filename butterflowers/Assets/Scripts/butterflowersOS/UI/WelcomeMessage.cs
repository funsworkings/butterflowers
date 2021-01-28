using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using uwu;

namespace butterflowersOS.UI
{
    public class WelcomeMessage : MonoBehaviour
    {
        World World;
        
        // Properties

        [SerializeField] TMP_Text text;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            World = World.Instance;
            while (!World.LOAD) yield return null;

            var username = World.username;
            DisplayUsername(username);
        }

        void DisplayUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) 
            {
                text.gameObject.SetActive(false);
                return;
            }
            
            text.text = username;
        }
    }
}
