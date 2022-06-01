using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// NOTE: EditorCoroutineが使えないため、無理のあるコードになっている。
    /// async/awaitが使える何かしらがUnity標準で使えるようになったら、それを利用して書き直したい。
    /// </summary>
    internal class Vrm10PackageInstaller
    {
        private static readonly string[] Packages =
        {
            "com.unity.burst",
            "com.unity.mathematics"
        };

        private int _installedIndex;
        private ListRequest _listRequest;
        private AddRequest _addRequest;

        private void ListUpPackagesUpdate()
        {
            if (_listRequest != null && _listRequest.IsCompleted)
            {
                // タスクが終わった場合
                if (_listRequest.Status == StatusCode.Success)
                {
                    // 含まれていないパッケージがあったらインストールするタスクを開始
                    if (!Packages.All(package => _listRequest.Result.Any(packageInfo => packageInfo.name == package)))
                    {
                        _installedIndex = 0;
                        EditorApplication.update += AddPackagesUpdate;
                    }
                    
                    _listRequest = null;
                    EditorApplication.update -= ListUpPackagesUpdate;
                    return;
                }

                if (_listRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError(_listRequest.Error.message);
                }

                return;
            }

            if (_listRequest == null)
            {
                // タスクが今実行されていない場合
                // 新たなタスクを開始する
                _listRequest = Client.List();

            }
        }

        private void AddPackagesUpdate()
        {
            if (_addRequest != null && _addRequest.IsCompleted)
            {
                // タスクが終わった場合
                // インストールされたことを示すメッセージをLogに出力
                if (_addRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Installed: " + _addRequest.Result.packageId);
                }
                else if (_addRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log("Install failure: " + _addRequest.Error.message);
                }

                _addRequest = null;
            }

            if (_addRequest == null)
            {
                // タスクが今実行されていない場合
                if (_installedIndex >= Packages.Length)
                {
                    // タスクが全て終わっていれば、終了
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update -= AddPackagesUpdate;
                    return;
                }

                EditorUtility.DisplayProgressBar("Install Packages", "", 0.5f);
                // または新たなタスクを開始する
                _addRequest = Client.Add(Packages[_installedIndex]);
                ++_installedIndex;
            }
        }

        [InitializeOnLoadMethod]
        private static void InstallPackages()
        {
            var packageInstaller = new Vrm10PackageInstaller();
            EditorApplication.update += packageInstaller.ListUpPackagesUpdate;
        }
    }
}