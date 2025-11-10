namespace UniGLTF
{
    public static class TestAssets
    {
        public static readonly string AssetPath = "Packages/com.vrmc.gltf/Tests/Objects";

        public static T LoadAsset<T>(string filename) where T : UnityEngine.Object
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>($"{AssetPath}/{filename}");
        }
    }
}