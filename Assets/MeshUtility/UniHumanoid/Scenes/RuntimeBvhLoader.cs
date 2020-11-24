using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniHumanoid;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniHumanoid
{
    public class RuntimeBvhLoader : MonoBehaviour
    {
        [SerializeField]
        Button m_openButton = default;

        [SerializeField]
        HumanPoseTransfer m_dst = default;

        UnityAction m_onClick;

        private void Awake()
        {
            m_onClick = new UnityEngine.Events.UnityAction(OnClick);
        }

        private void OnEnable()
        {
            m_openButton.onClick.AddListener(m_onClick);
        }

        private void OnDisable()
        {
            m_openButton.onClick.RemoveListener(m_onClick);
        }

        static string m_lastDir;

        public void OnClick()
        {
#if UNITY_EDITOR
            var path = EditorUtility.OpenFilePanel("open bvh", m_lastDir, "bvh");
            if (String.IsNullOrEmpty(path))
            {
                return;
            }
            m_lastDir = Path.GetDirectoryName(path);
#else
            string path=null;
#endif

#pragma warning disable 4014
            Open(path);
#pragma warning restore 4014
        }

        BvhImporterContext m_context;

        void Open(string path)
        {
            Debug.LogFormat("Open: {0}", path);
            if (m_context != null)
            {
                m_context.Destroy(true);
                m_context = null;
            }

            m_context = new BvhImporterContext();
            m_context.Parse(path);
            m_context.Load();

            var src = m_context.Root.AddComponent<HumanPoseTransfer>();

            if (m_dst != null)
            {
                m_dst.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;
                m_dst.Source = src;
            }
        }
    }
}
