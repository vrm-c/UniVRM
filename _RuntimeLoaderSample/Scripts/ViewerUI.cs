using System.Linq;
using UniHumanoid;
using UnityEngine;
using UnityEngine.UI;


namespace VRM
{
    public class ViewerUI : MonoBehaviour
    {
        #region UI
        [SerializeField]
        Text m_version;

        [SerializeField]
        Button m_open;

        [SerializeField]
        Toggle m_enableLipSync;

        [SerializeField]
        Toggle m_enableAutoBlink;
        #endregion

        [SerializeField]
        HumanPoseTransfer m_src;

        [SerializeField]
        GameObject m_target;

        private void Reset()
        {
            var buttons = GameObject.FindObjectsOfType<Button>();
            m_open = buttons.First(x => x.name == "Open");

            var toggles = GameObject.FindObjectsOfType<Toggle>();
            m_enableLipSync = toggles.First(x => x.name == "EnableLipSync");
            m_enableAutoBlink = toggles.First(x => x.name == "EnableAutoBlink");

            var texts= GameObject.FindObjectsOfType<Text>();
            m_version = texts.First(x => x.name == "Version");

            m_src = GameObject.FindObjectOfType<HumanPoseTransfer>();

            m_target = GameObject.FindObjectOfType<TargetMover>().gameObject;

        }

        GameObject m_loaded;

        AIUEO m_lipSync;
        bool m_enableLipSyncValue;
        bool EnableLipSyncValue
        {
            set
            {
                if (m_enableLipSyncValue == value) return;
                m_enableLipSyncValue = value;
                if (m_lipSync != null)
                {
                    m_lipSync.enabled = m_enableLipSyncValue;
                }
            }
        }

        Blinker m_blink;
        bool m_enableBlinkValue;
        bool EnableBlinkValue
        {
            set
            {
                if (m_blink == value) return;
                m_enableBlinkValue = value;
                if (m_blink != null)
                {
                    m_blink.enabled = m_enableBlinkValue;
                }
            }
        }

        private void Start()
        {
            m_version.text = string.Format("VRMViewer {0}.{1}", 
                VRMVersion.MAJOR, VRMVersion.MINOR);
            m_open.onClick.AddListener(OnOpenClicked);
        }

        private void Update()
        {
            EnableLipSyncValue = m_enableLipSync.isOn;
            EnableBlinkValue = m_enableAutoBlink.isOn;
        }

        void OnOpenClicked()
        {
            var path = FileDialogForWindows.FileDialog("open vrm", "vrm");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            Debug.LogFormat("{0}", path);

            var go = VRMImporter.LoadFromPath(path);
            if (go == null)
            {
                return;
            }

            SetModel(go);
        }

        void SetModel(GameObject go)
        {
            // cleanup
            if (m_loaded != null)
            {
                GameObject.Destroy(m_loaded);
            }

            m_loaded = go;

            var dst = go.AddComponent<HumanPoseTransfer>();
            dst.Source = m_src;
            dst.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;

            m_lipSync = go.AddComponent<AIUEO>();
            m_blink = go.AddComponent<Blinker>();

            var lookAt = go.GetComponent<VRMLookAtHead>();
            lookAt.Target = m_target.transform;
            lookAt.UpdateType = UpdateType.LateUpdate; // after HumanPoseTransfer's setPose
        }
    }
}
