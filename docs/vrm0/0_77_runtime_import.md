# RuntimeImport(0.77) RuntimeGltfInstance

[DisposeOnGameObjectDestroyed](https://github.com/vrm-c/UniVRM/issues/1018)

`ImporterContext` の仕様を変更しました。

`Version 0.68` で導入した、 `ImporterContext.DisposeOnGameObjectDestroyed` が扱いづらかったためのでこれを取りやめ、
`ImporterContext.Load` が `RuntimeGltfInstance` を返すようにしました。

`RuntimeGltfInstance` は、 `ImporterContext` の

* Root
* EnableUpdateWhenOffscreen()
* ShowMeshes()

を引き継ぎます。
Load の呼び出し後の任意のタイミングで ImporterContext.Dispose で Importer を破棄してください。
任意のタイミングで RuntimeGltfInstance を Destory することで紐づくリソース (Texture, Material, Mesh, etc) も破棄されます。

```csharp
using UniGLTF;
using UnityEngine;


namespace VRM.Samples
{
    public sealed class LoadVrmSample : MonoBehaviour
    {
        [SerializeField] private string _vrmFilePath;
        private GameObject _vrmGameObject;

        private void Start()
        {
            _vrmGameObject = LoadVrm(_vrmFilePath);
        }

        private void OnDestroy()
        {
            DestroyVrm(_vrmGameObject);
        }

        private GameObject LoadVrm(string vrmFilePath)
        {
            // 1. GltfParser を呼び出します。
            //    GltfParser はファイルから JSON 情報とバイナリデータを読み出します。
            var parser = new GltfParser();
            parser.ParsePath(vrmFilePath);

            // 2. GltfParser のインスタンスを引数にして VRMImporterContext を作成します。
            //    VRMImporterContext は VRM のロードを実際に行うクラスです。
            using (var context = new VRMImporterContext(parser))
            {
                // 3. Load 関数を呼び出し、VRM の GameObject を生成します。
                RuntimeGltfInstance instance = context.Load(); // <- `v0.77` でここが変わります。
                
                // 非同期版 async 関数の中で下記のようにしてください
                // RuntimeGltfInstance instance = await context.LoadAsync();

                // 4. （任意） SkinnedMeshRenderer の UpdateWhenOffscreen を有効にできる便利関数です。
                //    https://docs.unity3d.com/2019.4/Documentation/ScriptReference/SkinnedMeshRenderer-updateWhenOffscreen.html
                instance.EnableUpdateWhenOffscreen(); // <- ImporterContext から RuntimeGltfInstance に移動しました。

                // 5. VRM モデルを表示します。
                instance.ShowMeshes(); // <- ImporterContext から RuntimeGltfInstance に移動しました。

                // 6. Root の GameObject を return します。
                //    Root の GameObject とは VRMMeta コンポーネントが付与されている GameObject のことです。
                return instance.Root; // <- ImporterContext から RuntimeGltfInstance に移動しました。
            }
            // 7. using スコープを抜けて context が破棄されると、 VRMImporterContext が保持する UnityEngine.Object リソースが破棄されます。
            //    このとき破棄されるリソースは、 glTF ファイルには含まれているが VRM の GameObject には割り当てられていないテクスチャなどです。
            //    手順 6. で VRM の GameObject に紐付けたリソースは、ここでは破棄されません。
        }

        private void DestroyVrm(GameObject vrmGameObject)
        {
            // 8. 生成された VRM の GameObject を破棄します。
            //    GameObject を破棄すれば、紐づくリソース (Texture, Material, Mesh, etc) も破棄されます。
            UnityEngine.Object.Destroy(vrmGameObject);
        }
    }
}
```
