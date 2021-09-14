# glTF拡張の実装(0.63.2)

`UniVRM-0.63.2` から `UniGLTF` の構成が変わって、 `extensions` / `extras` の実装方法が変わりました。

## GLTF 拡張とは

`glTF` は各所に `extensions`, `extras` が定義してありその中身を拡張できます。

* `extensions` (またはextras)
* `asset.extensions` (またはextras)
* `meshes[*].extensions` (またはextras)
* `materials[*].extensions` (またはextras)

など。

`extensions` はオフィシャルに仕様を策定して `JsonSchema` として公開します。

* https://github.com/KhronosGroup/glTF/tree/master/extensions

`extensions` は、`{ベンダー名}_{拡張名}` という命名規則です。
ベンダー名は、 https://github.com/KhronosGroup/glTF に申し込んで登録できます。

`extras` は登録せずにアプリケーション独自に拡張する場合に用います。仕組みは同じです。

> This enables glTF models to contain application-specific properties without creating a full glTF extension

## UniGLTF の extensions

`v0.63.0` 以前は、`GLTF 型` の `extensions` フィールドに、`GLTFExtensions` 型を定義して、`VRM` フィールドを定義するという方法をとっていました。

```csharp
class VRM
{

}

class GLTFExtensions
{
    public VRM VRM;
}

class GLTF
{
    // すべての拡張の型をコンパイル時に知っている必要がある。動的に拡張できない
    public GLTFExtensions extensions;
}
```

この設計だと GLTF と拡張を別ライブラリとして分離することができませんでした。

`v0.63.1` から設計を変更して、すべての `extensions/extras` に同じ型の入れ物を使うように変更しました。
UniGLTF は `import/export` の具体的な内容を知らずに中間データの入れ物として扱います。

```csharp
// extensions / extras の入れ物として使う型
// 実行時は、 glTFExtensionImport / glTFExtensionExport を使う
public abstract class glTFExtension
{

}

class GLTF
{
    // UniGLTFは具体的な型を知らない。利用側が処理(serialize/deserialize)する
    public glTFExtension extensions;
}
```

## UniGLTF の拡張の書き方

拡張は、以下の部品要素から作れます。

* 名前(JsonPath)。例: `extensions.VRM`, `materials[*].extensions.KHR_materials_unlit`
* 拡張の型。`T型`
* デシリアライザー(import)。 `jsonバイト列 => T型`
* シリアライザーexport)。`T型 => jsonバイト列`

### JSONPATH と 型を決める

```C#
// 型
class GoodMaterial
{
    // `materials[*].extensions.CUSTOM_materials_good`
    public const string EXTENSION_NAME = "CUSTOM_materials_good";

    public int GoodValue;
}
```

### import

```C#
GoodMaterial DeserializeGoodMaterial(ListTreeNode<JsonValue> json)
{
    // デシリアライズ。手で書くかコード生成する(後述)
}

// ユーティリティ関数例
bool TryGetExtension<T>(UniGLTF.glTFExtension extension, string key, Func<ListTreeNode<JsonValue>, T> deserializer, out T value)
{
    if(material.extensions is UniGLTF.glTFExtensionsImport import)
    {
        // null check 完了
        foreach(var kv in import.ObjectItems())
        {
            if(kv.key.GetString()==key)
            {
                value = Deserialize(kv.Value);
                return true;
            }
        }
    }

    value = default;
    return false;
}

void ImportMaterial(UniGLTF.glTFMaterial material)
{
    // material の処理に割り込んで
    if(TryGetExtension(material.extension, GoodMaterial.EXTENSION_NAME, DeserializeGoodMaterial, out GoodMaterial good))
    {
        // good material 独自の処理
    }
}
```

### export

```csharp
void SerializeGoodMaterial(UniJSON.JsonFormatter f, GoodMaterial value)
{
    // シリアライズ。手で書くかコード生成する(後述)
}

// ユーティリティ関数例
public ArraySegment<byte> SerializeExtension<T>(T value, Func<T, ArraySegment<byte>> serialize)
{
    var f = new UniJSON.JsonFormatter();
    serialize(f, value);
    return f.GetStoreBytes();
}

void ExportGoodMaterial(UniGLTF.glTFMaterial material, GoodMaterial good)
{
    // material の処理に割り込んで
    if(!(material.extensions is UniGLTF.glTFExtensionsExport export))
    {
        // 無かった。新規作成
        export = new UniGLTF.glTFExtensionsExport();
        material.extensions = export;
    }

    var bytes = SerializeExtension(good, SerializeGoodMaterial);
    export.Add(GoodMaterial.EXTENSION_NAME, bytes);
}
```

## 実装例

### GLTF: GLTF全体
`C#の型からコード生成`

* `Assets\UniGLTF\Runtime\UniGLTF\Format\GltfSerializer.g.cs`
* `Assets\UniGLTF\Runtime\UniGLTF\Format\GltfDeserializer.g.cs`

ジェネレーターの呼び出しコード

* `Assets\UniGLTF\Editor\UniGLTF\Serialization\SerializerGenerator.cs`
* `Assets\UniGLTF\Editor\UniGLTF\Serialization\DeserializerGenerator.cs`

生成コードの呼び出し

### GLTF: `meshes[*].extras.targetNames`
`コード生成せずに手書き`

* `Assets\UniGLTF\Runtime\UniGLTF\Format\ExtensionsAndExtras\gltf_mesh_extras_targetNames.cs`

生成コードの呼び出し

### GLTF: `materials[*].extensions.KHR_materials_unlit`
`コード生成せずに手書き`

* `Assets\UniGLTF\Runtime\UniGLTF\Format\ExtensionsAndExtras\KHR_materials_unlit.cs`

生成コードの呼び出し

### GLTF: `materials[*].extensions.KHR_texture_transform`
`コード生成せずに手書き`

* `Assets\UniGLTF\Runtime\UniGLTF\Format\ExtensionsAndExtras\KHR_texture_transform.cs`

生成コードの呼び出し

* https://github.com/vrm-c/UniVRM/blob/master/Assets/UniGLTF/Runtime/UniGLTF/IO/MaterialImporter.cs#L296
* https://github.com/vrm-c/UniVRM/blob/master/Assets/UniGLTF/Runtime/UniGLTF/IO/MaterialExporter.cs#L193

### VRM0: `extensions.VRM`
`C#の型からコード生成`

* `Assets\VRM\Runtime\Format\VRMSerializer.g.cs`
* `Assets\VRM\Runtime\Format\VRMDeserializer.g.cs`

ジェネレーターの呼び出しコード

* `Assets\VRM\Editor\VRMSerializerGenerator.cs`
* `Assets\VRM\Editor\VRMDeserializerGenerator.cs`

生成コードの呼び出し

* https://github.com/vrm-c/UniVRM/blob/master/Assets/VRM/Runtime/IO/VRMImporterContext.cs#L41
* https://github.com/vrm-c/UniVRM/blob/master/Assets/VRM/Runtime/IO/VRMExporter.cs#L209

### VRM1: `extensions.VRMC_vrm` など
`JsonSchemaからコード生成`

5つの Extensions に分かれたので個別に作成。
ささる場所(JsonPath)が違うのに注意。

#### `extensions.VRMC_vrm`
* `Assets\VRM10\Runtime\Format\VRM`

#### `materials[*].extensions.VRMC_materials_mtoon`
* `Assets\VRM10\Runtime\Format\MaterialsMToon`

#### `nodes[*].extensions.VRMC_node_collider`
* `Assets\VRM10\Runtime\Format\NodeCollider`

#### `extensions.VRMC_springBone`
* `Assets\VRM10\Runtime\Format\SpringBone`

#### `extensions.VRMC_vrm_constraints`
* `Assets\VRM10\Runtime\Format\Constraints`

#### ジェネレーターの呼び出しコード
* `Assets\VRM10\Editor\GeneratorMenu.cs`

#### 生成コードの呼び出し

## コード生成
JSON と C# の型との シリアライズ/デシリアライズは定型コードになるので、ジェネレーターがあります。
C# の型から生成するものと、JsonSchema から C# の型とともに生成するものがあります。

### C# の型から生成

#### シリアライザー

ジェネレーターを呼び出すコードを作成します。

* 元になる型
* 出力先

の２つを決めます。static関数を生成するので、namespace と static class で囲ってあげます。

例

* `Assets\UniGLTF\Editor\UniGLTF\Serialization\SerializerGenerator.cs`

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UniJSON;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class SerializerGenerator
    {
        const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        const string Begin = @"// Don't edit manually. This is generaged. 
using System;
using System.Collections.Generic;
using UniJSON;

namespace UniGLTF {

    static public class GltfSerializer
    {

";

        const string End = @"
    } // class
} // namespace
";

        static string OutPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath,
                "UniGLTF/UniGLTF/Scripts/IO/GltfSerializer.g.cs");
            }
        }

        [MenuItem(UniGLTFVersion.MENU + "/GLTF: Generate Serializer")]
        static void GenerateSerializer()
        {
            var info = new ObjectSerialization(typeof(glTF), "gltf", "Serialize_");
            Debug.Log(info);

            using (var s = File.Open(OutPath, FileMode.Create))
            using (var w = new StreamWriter(s, new UTF8Encoding(false)))
            {
                w.Write(Begin);
                info.GenerateSerializer(w, "Serialize");
                w.Write(End);
            }

            Debug.LogFormat("write: {0}", OutPath);
            UnityPath.FromFullpath(OutPath).ImportAsset();
        }
    }
}
```

#### デシリアライザー

ジェネレーターを呼び出すコードを作成します。

* 元になる型
* 出力先

の２つを決めます。static関数を生成するので、namespace と static class で囲ってあげます。

例

* `Assets\UniGLTF\Editor\UniGLTF\Serialization\DeserializerGenerator.cs`

```csharp
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Generate deserializer from ListTreeNode<JsonValue> to glTF using type reflection
    /// </summary>
    public static class DeserializerGenerator
    {
        public const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        const string Begin = @"// Don't edit manually. This is generaged. 
using UniJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF {

public static class GltfDeserializer
{

";

        const string End = @"
} // GltfDeserializer
} // UniGLTF 
";

        static string OutPath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.dataPath,
                "UniGLTF/UniGLTF/Scripts/IO/GltfDeserializer.g.cs");
            }
        }

        [MenuItem(UniGLTFVersion.MENU + "/GLTF: Generate Deserializer")]
        static void GenerateSerializer()
        {
            var info = new ObjectSerialization(typeof(glTF), "gltf", "Deserialize_");
            Debug.Log(info);

            using (var s = File.Open(OutPath, FileMode.Create))
            using (var w = new StreamWriter(s, new UTF8Encoding(false)))
            {
                w.Write(Begin);
                info.GenerateDeserializer(w, "Deserialize");
                w.Write(End);
            }

            Debug.LogFormat("write: {0}", OutPath);
            UnityPath.FromFullpath(OutPath).ImportAsset();
        }
    }
}
```

#### キー出力の抑制

`index` に無効な値として `-1` を入れる場合に、JSONではキーを出力しないとしたいことがあります。

TODO: `int?` にするべきだった

```csharp
[JsonSchema(Minimum = 0)]
int index = -1;
```

のようにすることで、キーの出力を抑制できます。

```csharp
    // 生成コードのキー出力例
    if(value.index>=0){
```

何も付けないと

```csharp
    // 出力制御無し
    if(true){
```

#### enum のエンコーディング

enumの値の名前を文字列で使う、enumの値の数値を使うの2種類がありえます。
enumの場合はデフォルト値が無いので必須です。

```csharp
[JsonSchema(EnumSerializationType = EnumSerializationType.AsInt)]
public glBufferTarget target;

[JsonSchema(EnumSerializationType = EnumSerializationType.AsLowerString)]
public ProjectionType type;
```

### JsonSchemaから生成
VRM-1.0 の実装

TODO:
