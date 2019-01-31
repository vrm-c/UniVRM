using UnityEditor;


namespace VRM
{
    [CustomEditor(typeof(VRMFirstPerson))]
    class VRMFirstPersonEditor : Editor
    {
        void OnSceneGUI()
        {
            var component = target as VRMFirstPerson;

            var head = component.FirstPersonBone;
            if (head == null)
            {
                return;
            }


            var worldOffset = head.localToWorldMatrix.MultiplyPoint(component.FirstPersonOffset);
            worldOffset = Handles.PositionHandle(worldOffset, head.rotation);

            Handles.Label(worldOffset, "FirstPersonOffset");

            component.FirstPersonOffset = head.worldToLocalMatrix.MultiplyPoint(worldOffset);
        }
    }
}
