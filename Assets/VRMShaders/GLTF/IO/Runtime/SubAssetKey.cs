using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRMShaders
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
    public readonly struct SubAssetKey : IEquatable<SubAssetKey>
    {
        public static readonly Type TextureType = typeof(Texture);
        public static readonly Type MaterialType = typeof(Material);

        public readonly Type Type;
        public readonly string Name;

        public SubAssetKey(Texture obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.name))
            {
                throw new System.ArgumentNullException();
            }

            Type = TextureType;
            Name = obj.name;
        }

        public SubAssetKey(Material obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.name))
            {
                throw new System.ArgumentNullException();
            }

            Type = MaterialType;
            Name = obj.name;
        }

        public SubAssetKey(Type type, string name)
        {
            if (type == null || string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentNullException();
            }

            if (!type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                throw new System.ArgumentException($"{type}:{name}");
            }

            if (type.IsSubclassOf(TextureType))
            {
                type = TextureType;
            }

            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Type}:{Name}";
        }

        public override bool Equals(object obj)
        {
            if (obj is SubAssetKey key)
            {
                return this == key;
            }
            else
            {
                return true;
            }
        }

        public bool Equals(SubAssetKey other)
        {
            return Type == other.Type && Name == other.Name;
        }

        public static bool operator ==(SubAssetKey l, SubAssetKey r)
        {
            return l.Equals(r);
        }

        public static bool operator !=(SubAssetKey l, SubAssetKey r)
        {
            return !(l == r);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
