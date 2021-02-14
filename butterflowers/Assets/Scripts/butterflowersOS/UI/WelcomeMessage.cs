using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using uwu;
using uwu.Extensions;

namespace butterflowersOS.UI
{
    public class WelcomeMessage : MonoBehaviour
    {
        // Properties

        Animator animator;
        TMP_Text text;
        
        // Attributes

        [SerializeField] Color usernameColor = Color.white;

        void Awake()
        {
            animator = GetComponent<Animator>();
            text = GetComponentInChildren<TMP_Text>();
        }

        public void DisplayUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return;

            string colorHex = Extensions.ParseColor(usernameColor);
            text.text = string.Format("welcome back <color={0}>{1}</color>!", colorHex, username);
            
            animator.SetTrigger("show");
        }
    }
}
