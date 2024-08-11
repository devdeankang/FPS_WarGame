using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public SceneType TypeLoad;
    public List<Camera> cameraList;
    public Camera currentCamera;
    Camera mainCamera;

    List<GameObject> tests = new List<GameObject>();
    
    #region Singleton
    private static CameraManager instance = null;
    public static CameraManager Instance
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
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        SetTypeManager();
        InitCamera();
        currentCamera = mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void InitCamera()
    {
        if(cameraList == null || cameraList.Count == 0)
        {
            cameraList = new List<Camera>();
            var cams = GameObject.FindObjectsOfType<Camera>();
            
            foreach (var cam in cams) 
                cameraList.Add(cam);
        }
    }

    void SetTypeManager()
    {
        switch (TypeLoad)
        {
            case SceneType.Once:
                {
                    break;
                }
            case SceneType.DontDestroy:
                {
                    if (!instance)
                        DontDestroyOnLoad(this);
                    else Destroy(this);
                    break;
                }
        }
    }
}
