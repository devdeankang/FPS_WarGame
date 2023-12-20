using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Michsky.LSS;

public class SceneDataManager : MonoBehaviour
{
    public bool once { get; set; }
    [SerializeField] LoadingScreenManager lss = new LoadingScreenManager();

    string GalleryScene = "GalleryScene";

    private void Awake()
    {
        once = true;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private static SceneDataManager instance = null;
    public static SceneDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    public void LoadScene(string sceneName)
    {
        lss.LoadScene(sceneName);
    }

    private void Update()
    {
        // Test
        if(Input.GetKeyDown(KeyCode.F8))
        {
            once = false;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene == null && currentScene.name.Equals(GalleryScene))
        {
            once = false;
        }
    }
}
