using System.Collections.Generic;

namespace UniVRM10
{
    public interface IExternalUnityObject
    {
        Dictionary<string, T> GetExternalUnityObjects<T>() where T : UnityEngine.Object;
        void SetExternalUnityObject<T>(UnityEditor.AssetImporter.SourceAssetIdentifier sourceAssetIdentifier, T obj) where T : UnityEngine.Object;
    }
}


