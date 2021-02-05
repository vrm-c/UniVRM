namespace VrmLib
{
    public enum GeometryCoordinates
    {
        Unknown,

        /// VRM-0
        XYZ_RightUpBack_RH,

        /// VRM-1
        XYZ_RightUpForward_RH,

        /// D3D standard(Unity)
        XYZ_RightUpForward_LH,
    }

    public enum TextureOrigin
    {
        Unknown,

        // GLTF
        LeftTop,

        // Unity
        LeftBottom,
    }

    public struct Coordinates
    {
        public GeometryCoordinates Geometry;
        public TextureOrigin Texture;

        public static Coordinates Vrm0 => new Coordinates
        {
            Geometry = GeometryCoordinates.XYZ_RightUpBack_RH,
            Texture = TextureOrigin.LeftTop,
        };
        public bool IsVrm0 => this.Equals(Vrm0);

        public static Coordinates Vrm1 => new Coordinates
        {
            Geometry = GeometryCoordinates.XYZ_RightUpForward_RH,
            Texture = TextureOrigin.LeftTop,
        };
        public bool IsVrm1 => this.Equals(Vrm1);

        public static Coordinates Unity => new Coordinates
        {
            Geometry = GeometryCoordinates.XYZ_RightUpForward_LH,
            Texture = TextureOrigin.LeftBottom,
        };

        public bool IsUnity => this.Equals(Unity);
    }
}
