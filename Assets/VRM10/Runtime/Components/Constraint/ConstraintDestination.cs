using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    class ConstraintDestination
    {
        readonly Transform m_transform;
        readonly TRS m_modelInitial;
        readonly TRS m_localInitial;
        public readonly Transform ModelRoot;

        public ConstraintDestination(Transform t, Transform modelRoot = null)
        {
            ModelRoot = modelRoot;
            m_transform = t;

            m_localInitial = TRS.GetLocal(t);
            m_modelInitial = TRS.GetRelative(t, modelRoot.worldToLocalMatrix);
        }

        public void ApplyTranslation(Vector3 delta, float weight, ObjectSpace coords, Transform modelRoot = null)
        {
            switch (coords)
            {
                // case DestinationCoordinates.World:
                //     m_transform.position = value;
                //     break;

                case ObjectSpace.local:
                    {
                        var value = m_localInitial.Translation + delta * weight;
                        m_transform.localPosition = value;
                    }
                    break;

                case ObjectSpace.model:
                    {
                        var value = m_modelInitial.Translation + delta * weight;
                        m_transform.position = modelRoot.localToWorldMatrix.MultiplyPoint(value);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void ApplyRotation(Quaternion delta, float weight, ObjectSpace coords, Transform modelRoot = null)
        {
            // 0~1 で clamp しない slerp
            switch (coords)
            {
                // case DestinationCoordinates.World:
                //     m_transform.rotation = value;
                //     break;

                case ObjectSpace.local:
                    {
                        var value = Quaternion.LerpUnclamped(Quaternion.identity, delta, weight) * m_localInitial.Rotation;
                        m_transform.localRotation = value;
                    }
                    break;

                case ObjectSpace.model:
                    {
                        var value = Quaternion.LerpUnclamped(Quaternion.identity, delta, weight) * m_modelInitial.Rotation;
                        m_transform.rotation = modelRoot.rotation * value;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
