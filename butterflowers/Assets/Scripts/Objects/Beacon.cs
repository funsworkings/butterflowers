using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;
using UnityEngine.UI;

public class Beacon : MonoBehaviour
{
    #region External

    Navigator navigator = null;

    #endregion

    #region Properties

    [SerializeField] MeshRenderer thumbnailPanel;

    #endregion

    [SerializeField] string m_file = null;
    public string file
    {
        set
        {
            m_file = value;
        }
    }

    [SerializeField] Texture2D m_thumbnail;
    public Texture2D thumbnail
    {
        set
        {
            m_thumbnail = value;
            onUpdateThumbnail();
        }
    }

    void Awake() {
        navigator = FindObjectOfType<Navigator>();
    }

    void onUpdateThumbnail()
    {
        if (thumbnailPanel == null)
            return;

        thumbnailPanel.material.mainTexture = m_thumbnail;
    }

    void OnMouseDown()
    {
        navigator.Refresh(m_file); // Load file from beacon
    }

    void OnDestroy()
    {
        Texture2D.Destroy(m_thumbnail); // Dispose thumbnail image on destroy
    }
}
