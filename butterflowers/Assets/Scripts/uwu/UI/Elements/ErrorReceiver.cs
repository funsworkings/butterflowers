using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;

namespace UIExt.Elements {

public class ErrorReceiver : MonoBehaviour
{
    public static ErrorReceiver Instance = null;

    #region Events 

    public UnityEvent OnEnabled, OnDisabled;

    #endregion

    #region Collections

    List<Error> errors = new List<Error>(); 
    Error errorInFocus = null;

    #endregion

    #region Properties

    [SerializeField] TMP_Text errorTitleField, errorBodyField;

    #endregion

    #region Attributes

    [SerializeField] int errorsInQueue = 0;

    #endregion

    #region Monobehaviour callbacks

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Clear out Error receiver instance
        }
    }

    void OnEnable()
    {
        errors = new List<Error>();
        errorInFocus = null;

        OnDisabled.Invoke();
    }

    void Update()
    {
        errorsInQueue = errors.Count;

        if (errorInFocus == null)
            return;

        DisplayError(errorInFocus);
    }

    void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null; // Clear out previous instance
        }
    }

    #endregion

    public void Push(string title, string body, IErrorHandler handler)
    {
        string message = string.Format("{0}*{1}", title, body);
        System.Exception exception = new System.Exception(message);

        Push(exception, handler);
    }

    public void Push(System.Exception exception, IErrorHandler handler)
    {
        Error error = new Error(exception, handler);

        Push(error);
    }

    public void Push(Error error)
    {
        errors.Add(error); // Add to error queue
       
        if(errors.Count == 1)
        {
            OnEnabled.Invoke(); // Invoke enable to show error receiver
            errorInFocus = error; // Immediately pop first error in queue
        }
    }

    public void Pop()
    {
        errorInFocus = null;

        if(errors.Count > 0)
        {
            errors.RemoveAt(0);

            if(errors.Count > 0)
            {
                Error error = errors[0];
                errorInFocus = error;
            }
            else
            {
                OnDisabled.Invoke(); // Invoke disable to hide error receiver
            }
        }
    }

    public void Clear()
    {
        if (errorInFocus == null)
            return;

        errorInFocus.Dispose();
        Pop();
       //errorInFocus = null;
    }

    void onDisposeError(Error error)
    {
        if(errors.Contains(error))
            errors.Remove(error); // Remove from error collection

 //       error.onDispose -= onDisposeError;
        if (errors.Count == 0)
        {
            OnDisabled.Invoke(); // Invoke disable to hide error receiver
        }
        else
            Pop(); // Immediately 
    }

    void DisplayError(Error error)
    {
        string message = (error.exception != null)?error.exception.Message:"";
        string[] message_parts = message.Split('*');

        // Detected message with multiple astricks
        if(message_parts.Length > 2)
        {
            string message_title = message_parts[0];
            string message_body = "";

            string[] message_end = new string[message_parts.Length - 1];

                Array.Copy(message_parts, 1, message_end, 0, message_end.Length);
                message_body = string.Join("", message_end);
                
            message_parts = new string[2] { message_title, message_body };
        }

        if (message_parts.Length == 0)
            message_parts = new string[]{ "" };

        bool hasTitleAndBody = (message_parts.Length == 2);

        if (errorTitleField != null)
        {
            errorTitleField.enabled = true;
            if (hasTitleAndBody)
                errorTitleField.text = message_parts[0];
            else
                errorTitleField.text = "Error";
        }
        if (errorBodyField != null)
        {
            errorBodyField.enabled = true;
            errorBodyField.text = hasTitleAndBody ? message_parts[1] : message_parts[0];
        }
    }

    #region Helpers

    bool HandlerInQueue(IErrorHandler handler)
    {
        bool flag = false;
        for(int i = 0; i < errors.Count; i++)
        {
            if(errors[i].handler == handler)
            {
                flag = true;
                break;
            }
        }

        return flag;
    }

    #endregion
}

}
