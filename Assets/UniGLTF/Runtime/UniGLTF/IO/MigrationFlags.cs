namespace UniGLTF
{
    public sealed class MigrationFlags
    {
        /// <summary>
        /// Before UniGLTF v0.69, roughness value in the texture was invalid squared value.
        /// </summary>
        public bool IsRoughnessTextureValueSquared { get; set; } = false;
    }
}