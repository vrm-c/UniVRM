using System;


namespace UniGLTF
{
    /// <summary>
    /// UnityEditor.Experimental.AssetImporter.SourceAssetIdentifier に対応する
    /// 
    /// * SourceAssetIdentifier が UnityEditor なので、 Runtime でも使えるように
    /// * Type が違うが Name が同じだと警告が出る。例えば、Material と 同じ名前の Texture がある場合。
    /// Identifier uniqueness violation: 'Alicia_body'. Scripted Importers do not guarantee that subsequent imports of this asset will properly re-link to these targets.
    /// * なので、SourceAssetIdentifier は、$"{Type.Name}.{UnityObject.name}" のように強制的に Unique にするのが良さそう
    /// * 一方で、Extract したファイル名に $"{Type.Name}." が付属するのは煩わしいのでこれは無しにしたい。
    /// 
    /// public void AddRemap(SourceAssetIdentifier identifier, UnityEngine.Object externalObject);
    /// 
    /// の呼び出し時に、identifier.name と externalObject.name が同じでない運用にしてみる。
    /// 
    /// </summary>
    public struct SubAssetKey
    {
        public readonly Type Type;
        public readonly string Name;

        public SubAssetKey(Type t, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentNullException();
            }
            Type = t;
            Name = name;
        }

        // public bool Enable => Type != null;

        // /// <summary>
        // /// GetExternalObjectMap() の Key
        // /// </summary>
        // public string UnityObjectName => $"{Type.Name}.{Name}";

        // /// <summary>
        // /// Extract 先
        // /// </summary>
        // public string AssetFileName => $"{Name}{Ext}";

        // /// <summary>
        // /// CustomEditor用
        // /// </summary>
        // public string Label => Name;

        // public static SubAssetKey CreateMaterialKey(glTFMaterial material)
        // {
        //     return new SubAssetKey(typeof(Material), material.name, ".mat");
        // }

        // public static SubAssetKey CreateMaterialKey(Material material)
        // {
        //     return new SubAssetKey(typeof(Material), material.name, ".mat");
        // }

        // public static SubAssetKey CreateTextureKey(string name, string ext)
        // {
        //     return new SubAssetKey(typeof(Texture2D), name, ext);
        // }
    }
}

// ExtractKey = GetExtractKey(textureType, gltfName, ConvertedName, uri);

// public readonly string ExtractKey;

// public static string GetExtractKey(TextureImportTypes type, string gltfName, string convertedName, string uri)
// {
//     if (type == TextureImportTypes.StandardMap)
//     {
//         // metallic, smooth, occlusion
//         return convertedName;
//     }
//     else
//     {
//         if (!string.IsNullOrEmpty(uri))
//         {
//             // external image
//             return Path.GetFileNameWithoutExtension(uri);
//         }
//         else
//         {
//             // texture name
//             return gltfName;
//         }
//     }
// }
