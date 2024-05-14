using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace UniVRM10.FirstPersonSample
{
    public class VRM10CanvasManager : MonoBehaviour
    {
        [SerializeField]
        public Button LoadVRMButton;

        [SerializeField]
        public Button LoadBVHButton;

        private void Reset()
        {
#if UNITY_2022_3_OR_NEWER
            LoadVRMButton = GameObject.FindObjectsByType<Button>(FindObjectsSortMode.InstanceID).FirstOrDefault(x => x.name == "LoadVRM");
            LoadBVHButton = GameObject.FindObjectsByType<Button>(FindObjectsSortMode.InstanceID).FirstOrDefault(x => x.name == "LoadBVH");
#else
            LoadVRMButton = GameObject.FindObjectsOfType<Button>().FirstOrDefault(x => x.name == "LoadVRM");
            LoadBVHButton = GameObject.FindObjectsOfType<Button>().FirstOrDefault(x => x.name == "LoadBVH");
#endif
        }
    }
}
