using UnityEngine;


namespace VRM
{
    [CreateAssetMenu(menuName = "VRM/ExportObject")]
    public class VRMExportObject : ScriptableObject
    {
        [SerializeField]
        public VRMExportSettings Settings = new VRMExportSettings();
    }
}
