using UnityEngine;
using System;
using UniGLTF.Extensions.VRMC_constraints;

namespace UniVRM10
{
    class ConstraintSource
    {
        readonly Transform m_modelRoot;

        readonly Transform m_transform;

        readonly ObjectSpace m_coords;

        readonly TRS m_initial;

        public Vector3 TranslationDelta
        {
            get
            {
                switch (m_coords)
                {
                    // case ObjectSpace.World: return m_transform.position - m_initial.Translation;
                    case ObjectSpace.local: return m_transform.localPosition - m_initial.Translation;
                    case ObjectSpace.model: return m_modelRoot.worldToLocalMatrix.MultiplyPoint(m_transform.position) - m_initial.Translation;
                    default: throw new NotImplementedException();
                }
            }
        }

        public Quaternion RotationDelta
        {
            get
            {
                switch (m_coords)
                {
                    // 右からかけるか、左からかけるか、それが問題なのだ
                    // case SourceCoordinates.World: return m_transform.rotation * Quaternion.Inverse(m_initial.Rotation);
                    case ObjectSpace.local: return m_transform.localRotation * Quaternion.Inverse(m_initial.Rotation);
                    case ObjectSpace.model: return m_transform.rotation * Quaternion.Inverse(m_modelRoot.rotation) * Quaternion.Inverse(m_initial.Rotation);
                    default: throw new NotImplementedException();
                }
            }
        }

        public ConstraintSource(Transform t, ObjectSpace coords, Transform modelRoot = null)
        {
            m_transform = t;
            m_coords = coords;

            switch (coords)
            {
                // case SourceCoordinates.World:
                //     m_initial = TRS.GetWorld(t);
                //     break;

                case ObjectSpace.local:
                    m_initial = TRS.GetLocal(t);
                    break;

                case ObjectSpace.model:
                    {
                        var world = TRS.GetWorld(t);
                        m_modelRoot = modelRoot;
                        m_initial = new TRS
                        {
                            Translation = modelRoot.worldToLocalMatrix.MultiplyPoint(world.Translation),
                            Rotation = world.Rotation * Quaternion.Inverse(m_modelRoot.rotation),
                        };
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
