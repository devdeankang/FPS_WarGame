using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper;
using System.Text;

namespace Exhibition
{
    public class ArtBatchController : MonoBehaviour
    {
        public enum CurrentUI
        {
            None = 0,
            Artwork,
            Desc
        }

        public Dictionary<int, GameObject> arts = new Dictionary<int, GameObject>();
        public Dictionary<int, float> artDistance = new Dictionary<int, float>();
                
        public CurrentUI currentState = CurrentUI.None;
        GameObject player;

        private static ArtBatchController instance;
        public static ArtBatchController Instance
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
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            SetArtControllers();
            LoadArtData("Data.csv");
        }

        private void Update()
        {
            UpdatePlayerDistance();
            ControlMovement();
        }

        public void SetCurrentUIState(int state)
        {
            currentState = (CurrentUI)state;
        }

        void UpdatePlayerDistance()
        {
            foreach(var art in arts)
            {
                artDistance[art.Key] = (art.Value.transform.position - player.transform.position).sqrMagnitude;
            }            
        }

        void ControlMovement()
        {

            if (currentState != CurrentUI.None)
            {
                player.GetComponent<MoveController>().Freeze(true);
            }
            else
            {
                player.GetComponent<MoveController>().Freeze(false);
            }
        }

        void SetArtControllers()
        {
            foreach (Transform tr in transform)
            {
                tr.gameObject.AddComponent<ArtInteractController>();
                string[] splits = tr.gameObject.name.Split('_');
                int artOrder = int.Parse(splits[1]);
                tr.GetComponent<ArtInteractController>().artNum = artOrder;
                arts.Add(artOrder, tr.gameObject);
                artDistance.Add(artOrder, 0f);
            }
        }

        public bool LoadArtData(string csvName)
        {
            string filePath = string.Format("{0}/{1}/{2}",
                ConfigData.Instance().dataPath, ConfigData.Instance().artPath, csvName);

            if (!File.Exists(filePath))
                return false;

            CsvConfiguration csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8,
                Delimiter = ",",
                PrepareHeaderForMatch = args => args.Header.ToLower()
            };
            csvConfig.BadDataFound = null;
            
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(fs, Encoding.UTF8))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                var columns = csv.GetRecords<ArtData>();

                foreach (var col in columns)
                {
                    arts[col.order].GetComponent<ArtInteractController>().
                        SetArtData(col.id, col.name, col.order, col.format, col.desc);
                }                
            }

            return true;
        }
    }
}