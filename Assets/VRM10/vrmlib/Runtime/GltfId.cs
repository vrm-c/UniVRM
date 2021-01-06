namespace VrmLib
{
    /// <summary>
    /// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/glTFid.schema.json
    ///
    /// ImportとExportでなるべく順番を維持するべく導入。下記のベースクラスとした
    ///
    /// * Image, Texture, Material
    /// * Animation
    /// * Node, Skin, Mesh
    ///
    /// </summary>
    public class GltfId
    {
        public int? GltfIndex;

        /// <summary>
        /// 未指定は後ろに送る
        /// </summary>
        public int SortOrder => GltfIndex.GetValueOrDefault(int.MaxValue);
    }
}
