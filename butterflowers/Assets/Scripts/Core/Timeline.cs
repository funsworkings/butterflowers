using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UIExt;
using UIExt.Behaviors.Visibility;

public class Timeline: MonoBehaviour, IReactToSun {

    #region External

    GameDataSaveSystem Save = null;

    #endregion

    #region Properties

    [SerializeField] TMP_Text chapterTextField;
    [SerializeField] ToggleOpacity chapterDisplay;

	#endregion

	#region Attributes

	[SerializeField] int index = 0;
    [SerializeField] string chapter = "";
	[SerializeField] string[] chapters = new string[] { };

	#endregion

	IEnumerator Start()
    {
        Save = GameDataSaveSystem.Instance;
        while (!Save.load)
            yield return null;

        index = Save.chapter;
        if (index < 0) index = 0;

        chapter = chapters[index];
    }

    public void ReactToDay(int days)
    {
        if (chapters == null || chapters.Length == 0) return;

        index = days % chapters.Length;
        Save.chapter = index;
        chapter = chapters[index];

        //chapterTextField.text = chapter;
        //chapterDisplay.Show();
    }

    public void ReactToTimeOfDay(float timeOfDay)
    {
        // Do nothing
    }
}
