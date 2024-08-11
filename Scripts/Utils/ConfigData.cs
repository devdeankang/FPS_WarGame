using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigData
{
    public string dataPath = Application.dataPath;
    public string artPath = "Resources/ArtData";
    public string artMesh = "Mesh/art";
    
    static ConfigData instance;
    public static ConfigData Instance()
    {
        if(instance == null)
        {
            instance = new ConfigData();
        }
        return instance;
    }



}
