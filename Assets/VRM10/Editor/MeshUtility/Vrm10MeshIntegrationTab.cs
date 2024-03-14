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
    }
}