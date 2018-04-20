#pragma warning disable 0414, 0649
using UnityEngine;


namespace VRM
{
    public class VRMLookAtBlendShapeApplyer : MonoBehaviour, IVRMComponent
    {
        public bool DrawGizmo = true;

        [SerializeField, Header("Degree Mapping")]
        public CurveMapper Horizontal = new CurveMapper(90.0f, 1.0f);

        [SerializeField]
        public CurveMapper VerticalDown = new CurveMapper(90.0f, 1.0f);

        [SerializeField]
        public CurveMapper VerticalUp = new CurveMapper(90.0f, 1.0f);

        public void OnImported(VRMImporterContext context)
        {
            var gltfFirstPerson = context.VRM.extensions.VRM.firstPerson;
            Horizontal.Apply(gltfFirstPerson.lookAtHorizontalOuter);
            VerticalDown.Apply(gltfFirstPerson.lookAtVerticalDown);
            VerticalUp.Apply(gltfFirstPerson.lookAtVerticalUp);
        }

        VRMLookAtHead m_head;
        VRMBlendShapeProxy m_propxy;

        private void Start()
        {
            m_head = GetComponent<VRMLookAtHead>();
            m_propxy = GetComponent<VRMBlendShapeProxy>();
            if (m_head == null)
            {
                enabled = false;
                return;
            }
            m_head.YawPitchChanged += ApplyRotations;
        }

        void ApplyRotations(float yaw, float pitch)
        {
            if (yaw < 0)
            {
                // Left
                m_propxy.SetValue(BlendShapePreset.LookLeft, Horizontal.Map(-yaw));
                m_propxy.SetValue(BlendShapePreset.LookRight, 0);
            }
            else
            {
                // Right
                m_propxy.SetValue(BlendShapePreset.LookLeft, 0);
                m_propxy.SetValue(BlendShapePreset.LookRight, Horizontal.Map(yaw));
            }

            if (pitch < 0)
            {
                // Down
                m_propxy.SetValue(BlendShapePreset.LookUp, 0);
                m_propxy.SetValue(BlendShapePreset.LookDown, VerticalDown.Map(-pitch));
            }
            else
            {
                // Up
                m_propxy.SetValue(BlendShapePreset.LookUp, VerticalUp.Map(pitch));
                m_propxy.SetValue(BlendShapePreset.LookDown, 0);
            }
        }
    }
}
