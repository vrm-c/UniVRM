using UnityEngine;
using System;
using UniGLTF.Extensions.VRMC_node_constraint;

namespace UniVRM10
{
    class ConstraintSource
    {
        public readonly Transform ModelRoot;
        readonly Transform m_transform;
        readonly TRS m_modelInitial;
        readonly TRS m_localInitial;

        public Vector3 TranslationDelta(ObjectSpace coords)
        {
            switch (coords)
            {
                // case ObjectSpace.World: return m_transform.position - m_initial.Translation;
                case ObjectSpace.local: return m_transform.localPosition - m_localInitial.Translation;
                case ObjectSpace.model: return ModelRoot.worldToLocalMatrix.MultiplyPoint(m_transform.position) - m_modelInitial.Translation;
                default: throw new NotImplementedException();
            }
        }

        public Quaternion RotationDelta(ObjectSpace coords)
        {
            switch (coords)
            {
                // 右からかけるか、左からかけるか、それが問題なのだ
                // case SourceCoordinates.World: return m_transform.rotation * Quaternion.Inverse(m_initial.Rotation);
                case ObjectSpace.local: return m_transform.localRotation * Quaternion.Inverse(m_localInitial.Rotation);
                case ObjectSpace.model: return m_transform.rotation * Quaternion.Inverse(ModelRoot.rotation) * Quaternion.Inverse(m_modelInitial.Rotation);
                default: throw new NotImplementedException();
            }
        }

        public ConstraintSource(Transform t, Transform modelRoot = null)
        {
            m_transform = t;

            {
                m_localInitial = TRS.GetLocal(t);
            }

            {
                var world = TRS.GetWorld(t);
                ModelRoot = modelRoot;
                m_modelInitial = new TRS
                {
                    Translation = modelRoot.worldToLocalMatrix.MultiplyPoint(world.Translation),
                    Rotation = world.Rotation * Quaternion.Inverse(ModelRoot.rotation),
                };
            }
        }
    }
}
