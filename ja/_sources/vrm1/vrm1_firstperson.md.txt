# FirstPerson

{doc}`FirstPerson と Renderer の可視制御 </implementation/first_person>`

## Runtime に FirstPerson 機能を有効にする

VR向け FirstPerson 設定の初期化手順です。

1. Load する
2. Vrm10Instance を取得する
3. `controller.Vrm.FirstPerson.SetupAsync` を呼び出す
4. ShowMeshes

```csharp
async Task<RuntimeGltfInstance> LoadAsync(string path)
{
    var data = new GlbFileParser(path).Parse();
    if (!Vrm10Data.TryParseOrMigrate(data, true, out Vrm10Data vrm))
    {
        throw new System.Exception("vrm parse error !");
    }
    using (var loader = new Vrm10Importer(vrm))
    {
        // 1.
        var instance = await loader.LoadAsync();

        // 2.
        var controller = instance.GetComponent<Vrm10Instance>();

        // 3. The headless model that created is added to instance
        await controller.Vrm.FirstPerson.SetupAsync(controller.gameObject);

        // 4. 
        instance.ShowMeshes();

        return instance;
    }
}
```

## VRMの推奨する VR 向けのカメラ構成

ヘッドマウントディスプレイを表すカメラ と その他のカメラという２種類のカメラを想定ます。
それぞれに対して、

* FIRSTPERSON_ONLY_LAYER(このレイヤーを指定した gameObject はその他のカメラから消えます)
* THIRDPERSON_ONLY_LAYER(このレイヤーを指定した gameObject はヘッドマウントディスプレイから消えます)

を定義します。
これにより、ヘッドマウント視点のアバターの描画を抑止しつつ、他者からは見えるようにします。

例: アバターの頭の描画を抑止して前が見えるようにする

VRMは、`VRMFirstPersonOnly` と `VRMThirdPersonOnly` という名前のレイヤーを予約しています。

`Project Settings` - `Tags and Layers` に `VRMFirstPersonOnly` と `VRMThirdPersonOnly` を
設定してください。
サンプルでは、それぞれに `9` と `10` を割り当ています。

## 初期化時に layer を明示する

追加の引数で指定できます。

```csharp
var created = await controller.Vrm.FirstPerson.SetupAsync(controller.gameObject, firstPersonOnlyLayer: 9, thirdPersonOnlyLayer: 10);
```
