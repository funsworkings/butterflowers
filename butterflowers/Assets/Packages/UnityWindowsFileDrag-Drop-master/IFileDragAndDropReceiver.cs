using System.Collections.Generic;
using B83.Win32;
using UnityEngine;


public interface IFileDragAndDropReceiver
{
	void ReceiveFiles(List<string> files, POINT point);
}
