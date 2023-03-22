using System;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UniVRM10.URPSample
{
    public class UrpSampleUI : MonoBehaviour
    {
        [SerializeField] private Button openModelButton;

        private void Start()
        {
            openModelButton.onClick.AddListener(OnOpenModelButtonClicked);
        }

        private static async void OnOpenModelButtonClicked()
        {
            var path = EditorUtility.OpenFilePanel("Open VRM", "", "VRM");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                var vrm10Instance = await Vrm10.LoadPathAsync(path,
                    canLoadVrm0X: true,
                    showMeshes: false,
                    materialGenerator: new UrpVrm10MaterialDescriptorGenerator());
                if (vrm10Instance == null)
                {
                    return;
                }

                var instance = vrm10Instance.GetComponent<RuntimeGltfInstance>();
                instance.ShowMeshes();
                instance.EnableUpdateWhenOffscreen();
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
        }
    }
}