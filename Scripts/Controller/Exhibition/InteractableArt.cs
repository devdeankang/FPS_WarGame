using CsvHelper.Configuration.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Exhibition
{
    [RequireComponent(typeof(MeshCollider))]
    public class InteractableArt : MonoBehaviour
    {


    }

    [Serializable]
    public class ArtData
    {
        [Index(0)] public string id { get; set; }
        [Index(1)] public string name { get; set; }
        [Index(2)] public int order { get; set; }
        [Index(3)] public int format { get; set; }
        [Index(4)] public string desc { get; set; }
        
        //bool isValidData = true;
        public ArtData(string id = "", string name = "", int order = -1, int format = -1, string desc = "")
        {
            this.id = id;
            this.name = name;
            this.order = order;
            this.format = format;
            this.desc = desc;
        }
    }
}