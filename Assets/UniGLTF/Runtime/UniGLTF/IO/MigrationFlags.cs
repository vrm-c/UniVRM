namespace UniGLTF
{
    public sealed class MigrationFlags
    {
        /// <summary>
        /// Before UniGLTF v0.69, roughness value in the texture was invalid squared value.
        /// </summary>
        public bool IsRoughnessTextureValueSquared { get; set; } = false;

        /// <summary>
        /// Built-in RP Standard shader's emission color is exported in gamma color space until UniGLTF v0.106.0.
        /// </summary>
        public bool IsEmissiveFactorGamma { get; set; } = false;
    }
}