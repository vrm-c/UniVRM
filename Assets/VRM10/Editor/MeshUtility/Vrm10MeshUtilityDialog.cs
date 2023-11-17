using UnityEngine;
using UnityEditor;
using UniGLTF;

namespace UniVRM10
{
    public class Vrm10MeshUtilityDialog : UniGLTF.MeshUtility.MeshUtilityDialog
    {
        public new const string MENU_NAME = "VRM 1.0 MeshUtility";
        public new static void OpenWindow()
        {
            var window =
                (Vrm10MeshUtilityDialog)EditorWindow.GetWindow(typeof(Vrm10MeshUtilityDialog));
            window.titleContent = new GUIContent(MENU_NAME);
            window.Show();
        }
        protected override void Validate()
        {
            base.Validate();
            if (_exportTarget.GetComponent<Vrm10Instance>() == null)
            {
                _validations.Add(Validation.Error("target is not vrm1"));
                return;
            }
        }

        Vrm10MeshUtility _meshUtil;
        Vrm10MeshUtility Vrm10MeshUtility
        {
            get
            {
                if (_meshUtil == null)
                {
                    _meshUtil = new Vrm10MeshUtility();
                }
                return _meshUtil;
            }
        }
        protected override UniGLTF.MeshUtility.GltfMeshUtility MeshUtility => Vrm10MeshUtility;

        Vrm10MeshIntegrationTab _integrationTab;
        protected override UniGLTF.MeshUtility.MeshIntegrationTab MeshIntegration
        {
            get
            {
                if (_integrationTab == null)
                {
                    _integrationTab = new Vrm10MeshIntegrationTab(this, Vrm10MeshUtility);
                }
                return _integrationTab;
            }
        }

        protected override bool MeshIntegrateGui()
        {
            var firstPerson = ToggleIsModified("FirstPerson == AUTO の生成", ref MeshUtility.GenerateMeshForFirstPersonAuto);
            var mod = base.MeshIntegrateGui();
            return firstPerson || mod;
        }
    }
}