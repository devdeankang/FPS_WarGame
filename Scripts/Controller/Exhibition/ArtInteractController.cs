using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Exhibition
{
    public class ArtInteractController : MonoBehaviour
    {


        public int artNum;  // order
        public ArtData data;
        List<MeshCollider> colls = new List<MeshCollider>();
        List<MeshFilter> filters = new List<MeshFilter>();
        
        

        private void Start()
        {
            
            SetMeshData();
            LoadArt();
        }

        public void SetArtData(string id, string name, int order, int format, string desc)
        {
            data = new ArtData(id, name, order, format, desc);
        }

        void LoadArt()
        {
            string directory = ConfigData.Instance().artPath;
            string path = directory.Substring(directory.IndexOf("/")+1);
            string filePath = string.Format("{0}/{1}", path, data.id);
            
            var texture = Resources.Load(filePath, typeof(Texture2D)) as Texture2D;
            this.GetComponent<MeshRenderer>().material.mainTexture = texture;
            this.GetComponent<MeshCollider>().sharedMesh = colls[data.format].sharedMesh;
            this.GetComponent<MeshFilter>().sharedMesh = filters[data.format].sharedMesh;
        }

        void SetMeshData()
        {
            string directory = ConfigData.Instance().artPath;
            string path = directory.Substring(directory.IndexOf("/")+1);
            string meshPath = string.Format("{0}/{1}", path, ConfigData.Instance().artMesh);

            colls.Add(new MeshCollider());
            filters.Add(new MeshFilter());

            Transform mesh = Resources.Load(meshPath, typeof(Transform)) as Transform;
            foreach(Transform tr in mesh.transform)
            {
                colls.Add(tr.gameObject.GetComponent<MeshCollider>());
                filters.Add(tr.gameObject.GetComponent<MeshFilter>());
            }
        }
    }
}