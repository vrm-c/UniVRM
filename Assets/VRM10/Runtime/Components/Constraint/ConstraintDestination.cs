using System;
using UnityEngine;

namespace UniVRM10
{
    public enum DestinationCoordinates
    {
        World,
        Local,
    }

    class ConstraintDestination
    {
        readonly Transform m_transform;
        readonly DestinationCoordinates m_coords;

        readonly TRS m_initial;

        public ConstraintDestination(Transform t, DestinationCoordinates coords)
        {
            m_transform = t;
            m_coords = coords;

            switch (m_coords)
            {
                case DestinationCoordinates.World:
                    m_initial = TRS.GetWorld(t);
                    break;

                case DestinationCoordinates.Local:
                    m_initial = TRS.GetLocal(t);
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
                case DestinationCoordinates.World:
                    m_transform.position = value;
                    break;

                case DestinationCoordinates.Local:
                    m_transform.localPosition = value;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void ApplyRotation(Quaternion delta, float weight)
        {
            // 0~1 で clamp しない slerp
            var value = Quaternion.LerpUnclamped(Quaternion.identity, delta, weight) * m_initial.Rotation;
            switch (m_coords)
            {
                case DestinationCoordinates.World:
                    m_transform.rotation = value;
                    break;

                case DestinationCoordinates.Local:
                    m_transform.localRotation = value;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
