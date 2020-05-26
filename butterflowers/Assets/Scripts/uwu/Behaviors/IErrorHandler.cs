using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IErrorHandler
{
    void OnReceiveError(System.Exception err);
    void OnResolveError();
}
