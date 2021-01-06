using UnityEngine;
using System;

namespace UniVRM10
{
    public enum SourceCoordinates
    {
        /// <summary>
        /// ワールド座標
        /// </summary>
        World,

        /// <summary>
        /// モデルルート(指定のTransform)ローカル座標
        /// </summary>
        Model,

        /// <summary>
        /// m_transform ローカル座標
        /// </summary>
        Local,
    }

    class ConstraintSource
    {
        readonly Transform m_modelRoot;

        readonly Transform m_transform;

        readonly SourceCoordinates m_coords;

        readonly TRS m_initial;

        public Vector3 TranslationDelta
        {
            get
            {
                switch (m_coords)
                {
                    case SourceCoordinates.World: return m_transform.position - m_initial.Translation;
                    case SourceCoordinates.Local: return m_transform.localPosition - m_initial.Translation;
                    case SourceCoordinates.Model: return m_modelRoot.worldToLocalMatrix.MultiplyPoint(m_transform.position) - m_initial.Translation;
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
                    case SourceCoordinates.World: return m_transform.rotation * Quaternion.Inverse(m_initial.Rotation);
                    case SourceCoordinates.Local: return m_transform.localRotation * Quaternion.Inverse(m_initial.Rotation);
                    case SourceCoordinates.Model: return m_transform.rotation * Quaternion.Inverse(m_modelRoot.rotation) * Quaternion.Inverse(m_initial.Rotation);
                    default: throw new NotImplementedException();
                }
            }
        }

        public ConstraintSource(Transform t, SourceCoordinates coords, Transform modelRoot = null)
        {
            m_transform = t;
            m_coords = coords;

            switch (coords)
            {
                case SourceCoordinates.World:
                    m_initial = TRS.GetWorld(t);
                    break;

                case SourceCoordinates.Local:
                    m_initial = TRS.GetLocal(t);
                    break;

                case SourceCoordinates.Model:
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
