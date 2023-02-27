using UnityEngine;

namespace UniGLTF.Utils
{
    public readonly struct EuclideanTransform
    {
        public readonly Quaternion Rotation;
        public readonly Vector3 Translation;

        public EuclideanTransform(Quaternion rotation, Vector3 translation)
        {
            Rotation = rotation;
            Translation = translation;
        }
        public EuclideanTransform(Quaternion rotation)
        {
            Rotation = rotation;
            Translation = Vector3.zero;
        }
        public EuclideanTransform(Vector3 translation)
        {
            Rotation = Quaternion.identity;
            Translation = translation;
        }
        public EuclideanTransform(Matrix4x4 matrix)
        {
            Rotation = matrix.rotation;
            Translation = matrix.GetColumn(3);
        }
    }
}
