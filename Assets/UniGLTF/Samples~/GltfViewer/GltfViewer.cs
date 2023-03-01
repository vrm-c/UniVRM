using System;
using UnityEngine;

namespace UniGLTF.GltfViewer
{
    public class GltfViewer : MonoBehaviour
    {
        RuntimeGltfInstance _instance;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnOpenClicked()
        {
            var path = OpenFileDialog.Show("open gltf", "glb", "gltf", "zip");
            if (!path.Exists)
            {
                return;
            }
            Debug.Log($"open: {path}");

            LoadPathAsync(path);
        }

        async void LoadPathAsync(VRMShaders.PathObject path)
        {
            if (_instance)
            {
                // clear prev
                _instance.Dispose();
                _instance = null;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                _instance = await GltfUtility.LoadAsync(path.FullPath);
                if (_instance == null)
                {
                    Debug.LogWarning("LoadAsync: null");
                    return;
                }
                Debug.Log($"LoadAsync: {sw.Elapsed}");
                _instance.ShowMeshes();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
