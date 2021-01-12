using System.Numerics;

namespace VrmLib
{
    public class TextureInfo
    {

        public Texture Texture;
        // uv = uv * scaling + offset
        public Vector2 Offset;
        public Vector2 Scaling = Vector2.One;

        public float[] OffsetScaling
        {
            get => new float[] { Offset.X, Offset.Y, Scaling.X, Scaling.Y };
            set
            {
                Offset.X = value[0];
                Offset.Y = value[1];
                Scaling.X = value[2];
                Scaling.Y = value[3];
            }
        }

        public TextureInfo(Texture texture)
        {
            Texture = texture;
        }
    }
}