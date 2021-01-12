namespace VrmLib
{
    public enum GeometryCoordinates
    {
        Unknown,

        /// OpenGL standard
        XYZ_RightUpBack_RH,

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

        public static Coordinates Gltf => new Coordinates
        {
            Geometry = GeometryCoordinates.XYZ_RightUpBack_RH,
            Texture = TextureOrigin.LeftTop,
        };

        public bool IsGltf => this.Equals(Gltf);

        public static Coordinates Unity => new Coordinates
        {
            Geometry = GeometryCoordinates.XYZ_RightUpForward_LH,
            Texture = TextureOrigin.LeftBottom,
        };

        public bool IsUnity => this.Equals(Unity);
    }
}
