using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    class ConstraintDestination
    {
        readonly Transform m_transform;
        readonly ObjectSpace m_coords;

        readonly TRS m_initial;

        public ConstraintDestination(Transform t, ObjectSpace coords, Transform modelRoot = null)
        {
            m_transform = t;
            m_coords = coords;

            switch (m_coords)
            {
                // case ObjectSpace.World:
                //     m_initial = TRS.GetWorld(t);
                //     break;

                case ObjectSpace.local:
                    m_initial = TRS.GetLocal(t);
                    break;

                case ObjectSpace.model:
                    m_initial = TRS.GetRelative(t, modelRoot.worldToLocalMatrix);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void ApplyTranslation(Vector3 delta, float weight)
        {
            var value = m_initial.Translation + delta * weight;
            switch (m_coords)
            {
                // case DestinationCoordinates.World:
                //     m_transform.position = value;
                //     break;

                case ObjectSpace.local:
                    m_transform.localPosition = value;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void ApplyRotation(Quaternion delta, float weight, Transform modelRoot = null)
        {
            // 0~1 で clamp しない slerp
            var value = Quaternion.LerpUnclamped(Quaternion.identity, delta, weight) * m_initial.Rotation;
            switch (m_coords)
            {
                // case DestinationCoordinates.World:
                //     m_transform.rotation = value;
                //     break;

                case ObjectSpace.local:
                    m_transform.localRotation = value;
                    break;

                case ObjectSpace.model:
                    m_transform.rotation = modelRoot.rotation * value;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
