using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    class Vrm10MeshIntegrationTab : UniGLTF.MeshUtility.MeshIntegrationTab
    {
        Vrm10MeshUtility _vrmMeshUtil;

        public Vrm10MeshIntegrationTab(EditorWindow editor, Vrm10MeshUtility meshUtility) : base(editor, meshUtility)
        {
            _vrmMeshUtil = meshUtility;
        }

        public override void UpdateMeshIntegrationList(GameObject root)
        {
            _selected = -1;
            _meshUti.MeshIntegrationGroups.Clear();
            _vrmMeshUtil.IntegrateFirstPerson(root);
            Selected = 0;
        }
    }
}