using System;

namespace UniGLTF
{
    [Serializable]
    public struct SkinJoints : IEquatable<SkinJoints>
    {
        public ushort Joint0;
        public ushort Joint1;
        public ushort Joint2;
        public ushort Joint3;

        public SkinJoints(ushort j0, ushort j1, ushort j2, ushort j3)
        {
            Joint0 = j0;
            Joint1 = j1;
            Joint2 = j2;
            Joint3 = j3;
        }

        public bool Equals(SkinJoints other)
        {
            if (Joint0 != other.Joint0) return false;
            if (Joint1 != other.Joint1) return false;
            if (Joint2 != other.Joint2) return false;
            if (Joint3 != other.Joint3) return false;
            return true;
        }
    }
}
