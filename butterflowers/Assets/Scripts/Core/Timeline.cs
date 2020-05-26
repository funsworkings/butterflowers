using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline: MonoBehaviour, IReactToSun {

    #region External

    [SerializeField] GameDataSaveSystem Save = null;

    #endregion

    [SerializeField] int index = 0;
    [SerializeField] string chapter = "";
	[SerializeField] string[] chapters = new string[] { };

    IEnumerator Start()
    {
        while (!Save.load)
            yield return null;

        index = Save.chapter;
        if (index < 0) index = 0;

        chapter = chapters[index];
    }

    public void ReactToDay(int days)
    {
        if (chapters == null || chapters.Length == 0) return;

        int len = chapters.Length;
        if (len > 1) 
        {
            index = (days % (chapters.Length - 1));
        }
        else 
        {
            index = 0;
        }
        
        Save.chapter = index;

        chapter = chapters[index];
        onMoveToNextChapter();
    }

    public void ReactToTimeOfDay(float timeOfDay)
    {
        
    }

    void onMoveToNextChapter()
    {
        
    }
}
