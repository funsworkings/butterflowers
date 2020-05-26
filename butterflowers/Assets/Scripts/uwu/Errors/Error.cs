using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Error
{
    #region Events

    public System.Action<Error> onDispose;

    #endregion

    public string message = "";

    [HideInInspector] public System.Exception exception;
    [HideInInspector] public IErrorHandler handler;

    public Error(System.Exception exception, IErrorHandler handler)
    {
        this.exception = exception;
        this.handler = handler;

        this.message = (exception != null) ? exception.Message : "";
    }

    #region Operations

    public void Dispose()
    {
        if (handler != null)
            handler.OnResolveError();

        if (onDispose != null)
            onDispose(this);
    }

    #endregion
}
