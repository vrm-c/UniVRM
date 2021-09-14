# RuntimeImport(0.68)

## 過去バージョンからの仕様変更

`ImporterContext` の仕様を変更しました。

* ロード処理が Parse と Load の 2 ステップに分かれました。
    * Parse 処理をメインスレッド以外で処理することができます。
* 非同期ロード関数 `ImporterContext.LoadAsync` の実装を `Task` に変更しました。
* これまで明示的に破棄できなかった `UnityEngine.Object` リソースを破棄できるようになりました。
    * リソースのリークを防ぐことができます。
* `ImporterContext.Dispose` を呼び出すべきタイミングを「ロード処理終了時」に変更しました。
    * 呼び出して破棄する前に、後述の `ImporterContext.DisposeOnGameObjectDestroyed` を呼び出してください。
    * 以前の仕様は「生成したモデルの破棄時」に呼び出すべき関数でした。
* `ImporterContext.DisposeOnGameObjectDestroyed` 関数を追加しました。
    * VRM モデルが必要とするリソース (Texture, Material, Mesh, etc) を破棄する責務を GameObject に移譲できます。
    * VRM の GameObject の破棄タイミングでリソース (Texture, Material, Mesh, etc) を破棄します。


## サンプルコード（同期的ロード）

```csharp
using UniGLTF;
using UnityEngine;
using VRM;

namespace YourNameSpace
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
                context.Load();

                // 4. （任意） SkinnedMeshRenderer の UpdateWhenOffscreen を有効にできる便利関数です。
                //    https://docs.unity3d.com/2019.4/Documentation/ScriptReference/SkinnedMeshRenderer-updateWhenOffscreen.html
                context.EnableUpdateWhenOffscreen();

                // 5. VRM モデルを表示します。
                context.ShowMeshes();

                // 6. VRM の GameObject が実際に使用している UnityEngine.Object リソースの寿命を VRM の GameObject に紐付けます。
                //    つまり VRM の GameObject の破棄時に、実際に使用しているリソース (Texture, Material, Mesh, etc) をまとめて破棄することができます。
                context.DisposeOnGameObjectDestroyed();

                // 7. Root の GameObject を return します。
                //    Root の GameObject とは VRMMeta コンポーネントが付与されている GameObject のことです。
                return context.Root;
            }
            // 8. using スコープを抜けて context が破棄されると、 VRMImporterContext が保持する UnityEngine.Object リソースが破棄されます。
            //    このとき破棄されるリソースは、 glTF ファイルには含まれているが VRM の GameObject には割り当てられていないテクスチャなどです。
            //    手順 6. で VRM の GameObject に紐付けたリソースは、ここでは破棄されません。
        }

        private void DestroyVrm(GameObject vrmGameObject)
        {
            // 9. 生成された VRM の GameObject を破棄します。
            //    GameObject を破棄すれば、紐づくリソース (Texture, Material, Mesh, etc) も破棄されます。
            UnityEngine.Object.Destroy(vrmGameObject);
        }
    }
}
```

## サンプルコード（非同期ロード）

```csharp
using System.IO;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRM;

namespace YourNameSpace
{
    public sealed class LoadVrmAsyncSample : MonoBehaviour
    {
        [SerializeField] private string _vrmFilePath;
        private GameObject _vrmGameObject;

        private async void Start()
        {
            // 簡便のため、このサンプルではキャンセル処理などは考慮しません。
            _vrmGameObject = await LoadVrmAsync(_vrmFilePath);
        }

        private void OnDestroy()
        {
            DestroyVrm(_vrmGameObject);
        }

        private async Task<GameObject> LoadVrmAsync(string vrmFilePath)
        {
            // 1. GltfParser を呼び出します。
            //    GltfParser はファイルから JSON 情報とバイナリデータを読み出します。
            //    GltfParser は Unity のメインスレッド以外で実行できます。
            var parser = new GltfParser();
            await Task.Run(() =>
            {
                var file = File.ReadAllBytes(vrmFilePath);
                parser.ParseGlb(file);
            });

            // 2. GltfParser のインスタンスを引数にして VRMImporterContext を作成します。
            //    VRMImporterContext は VRM のロードを実際に行うクラスです。
            using (var context = new VRMImporterContext(parser))
            {
                // 3. Load 関数を呼び出し、VRM の GameObject を生成します。
                //    Load 処理は数フレームの時間を要します。
                await context.LoadAsync();

                // 4. （任意） SkinnedMeshRenderer の UpdateWhenOffscreen を有効にできる便利関数です。
                //    https://docs.unity3d.com/2019.4/Documentation/ScriptReference/SkinnedMeshRenderer-updateWhenOffscreen.html
                context.EnableUpdateWhenOffscreen();

                // 5. VRM モデルを表示します。
                context.ShowMeshes();

                // 6. VRM の GameObject が実際に使用している UnityEngine.Object リソースの寿命を VRM の GameObject に紐付けます。
                //    つまり VRM の GameObject の破棄時に、実際に使用しているリソース (Texture, Material, Mesh, etc) をまとめて破棄することができます。
                context.DisposeOnGameObjectDestroyed();

                // 7. Root の GameObject を return します。
                //    Root の GameObject とは VRMMeta コンポーネントが付与されている GameObject のことです。
                return context.Root;
            }
            // 8. using スコープを抜けて context が破棄されると、 VRMImporterContext が保持する UnityEngine.Object リソースが破棄されます。
            //    このとき破棄されるリソースは、 glTF ファイルには含まれているが VRM の GameObject には割り当てられていないテクスチャなどです。
            //    手順 6. で VRM の GameObject に紐付けたリソースは、ここでは破棄されません。
        }

        private void DestroyVrm(GameObject vrmGameObject)
        {
            // 9. 生成された VRM の GameObject を破棄します。
            // GameObject を破棄すれば、紐づくリソース (Texture, Material, Mesh, etc) も破棄されます。
            UnityEngine.Object.Destroy(vrmGameObject);
        }
    }
}
```
