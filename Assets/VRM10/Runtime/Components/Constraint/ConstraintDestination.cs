using System;
using UniGLTF.Extensions.VRMC_node_constraint;
using UnityEngine;

namespace UniVRM10
{
    public class ConstraintDestination
    {
        readonly Transform m_transform;
        public readonly TR ModelInitial;
        public readonly TR LocalInitial;
        public readonly Transform ModelRoot;

        public ConstraintDestination(Transform t, Transform modelRoot = null)
        {
            ModelRoot = modelRoot;
            m_transform = t;

            LocalInitial = TR.FromLocal(t);
            ModelInitial = TR.FromRelative(t, modelRoot);
        }

        public void ApplyTranslation(Vector3 delta, float weight, ObjectSpace coords, Quaternion offset, Transform modelRoot = null)
        {
            switch (coords)
            {
                // case DestinationCoordinates.World:
                //     m_transform.position = value;
                //     break;

                case ObjectSpace.local:
                    {
                        var value = LocalInitial.Translation + offset * delta * weight;
                        m_transform.localPosition = value;
                    }
                    break;

                case ObjectSpace.model:
                    {
                        var value = ModelInitial.Translation + offset * delta * weight;
                        m_transform.position = modelRoot.localToWorldMatrix.MultiplyPoint(value);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void ApplyRotation(Quaternion delta, float weight, ObjectSpace coords, Quaternion offset, Transform modelRoot = null)
        {
            // 0~1 で clamp しない slerp
            switch (coords)
            {
                // case DestinationCoordinates.World:
                //     m_transform.rotation = value;
                //     break;

                case ObjectSpace.local:
                    {
                        var value = Quaternion.LerpUnclamped(Quaternion.identity, delta * offset, weight) * LocalInitial.Rotation;
                        m_transform.localRotation = value;
                    }
                    break;

                case ObjectSpace.model:
                    {
                        var value = Quaternion.LerpUnclamped(Quaternion.identity, delta * offset, weight) * ModelInitial.Rotation;
                        m_transform.rotation = modelRoot.rotation * value;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
