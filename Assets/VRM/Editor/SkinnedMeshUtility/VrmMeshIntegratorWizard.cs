#pragma warning disable 0414, 0649
using UnityEditor;
using UnityEngine;
using UniGLTF.M17N;
using UniGLTF;
using System.Collections.Generic;


namespace VRM
{
    public class VrmMeshIntegratorWizard : UniGLTF.MeshUtility.MeshUtilityDialog
    {
        public new const string MENU_NAME = "VRM 0.x MeshUtility";
        public new static void OpenWindow()
        {
            var window =
                (VrmMeshIntegratorWizard)EditorWindow.GetWindow(typeof(VrmMeshIntegratorWizard));
            window.titleContent = new GUIContent(MENU_NAME);
            window.Show();
        }
        protected override void Validate()
        {
            base.Validate();
            if (_exportTarget.GetComponent<VRMMeta>() == null)
            {
                _validations.Add(Validation.Error("target is not vrm1"));
                return;
            }
        }

        VrmMeshUtility _meshUtil;
        VrmMeshUtility VrmMeshUtility
        {
            get
            {
                if (_meshUtil == null)
                {
                    _meshUtil = new VrmMeshUtility();
                }
                return _meshUtil;
            }
        }
        protected override UniGLTF.MeshUtility.GltfMeshUtility MeshUtility => VrmMeshUtility;

        protected override bool MeshIntegrateGui()
        {
            var firstPerson = ToggleIsModified("FirstPerson == AUTO の生成", ref MeshUtility.GenerateMeshForFirstPersonAuto);
            var mod = base.MeshIntegrateGui();
            return firstPerson || mod;
        }

        List<BlendShapeClip> _clips;
        protected override void WriteAssets(string assetFolder,
            GameObject instance, List<UniGLTF.MeshUtility.MeshIntegrationResult> results)
        {
            // 統合した結果を反映した BlendShapeClip を作成して置き換える
            _clips = VrmBlendShapeUpdater.FollowBlendshapeRendererChange(assetFolder, instance, results);

            // write mesh
            base.WriteAssets(assetFolder, instance, results);

            // reset firstPerson
            if (instance.GetComponent<VRMFirstPerson>() is VRMFirstPerson firstPerson)
            {
                // TODO:
                firstPerson.Reset();
            }
        }

        protected override string WritePrefab(string assetFolder,
            GameObject instance)
        {
            var prefabPath = base.WritePrefab(assetFolder, instance);

            // PostProcess
            // update prefab reference of BlendShapeClip
            var prefabReference = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            foreach (var clip in _clips)
            {
                var so = new SerializedObject(clip);
                so.Update();
                var prop = so.FindProperty("m_prefab");
                prop.objectReferenceValue = prefabReference;
                so.ApplyModifiedProperties();
            }

            return prefabPath;
        }

        protected override void DialogMessage()
        {
            EditorGUILayout.HelpBox(Message.MESH_UTILITY.Msg(), MessageType.Info);
        }

        enum Message
        {
            [LangMsg(Languages.ja, @"(VRM-0.x専用) 凍結 > 統合 > 分割 という一連の処理を実行します。

[凍結]
- ヒエラルキーの 回転・拡縮を Mesh に焼き付けます。
- BlendShape の現状を Mesh に焼き付けます。

- VRM-0.x の正規化処理です。
- HumanoidAvatar の再生成。
- BlendShapeClip, SpringBone, Constraint なども影響を受けます。

[統合]
- ヒエラルキーに含まれる MeshRenderer と SkinnedMeshRenderer をひとつの SkinnedMeshRenderer に統合します。

- VRM の FirstPerson 設定に応じて３種類(BOTH, FirstPerson, ThirdPerson) にグループ化して統合します。
- FirstPerson=AUTO を前処理できます。
    - 元の Mesh は ThirdPerson として処理されます。頭なしのモデルを追加生成して FirstPersonOnly とします。

[分割]
- 統合結果を BlendShape の有無を基準に分割します。
- BOTH, FirstPerson, ThirdPerson x 2 で、最大で 6Mesh になります。空の部分ができることが多いので 3Mesh くらいが多くなります。

[Scene と Prefab]
Scene と Prefab で挙動が異なります。

(Scene/Runtime)
- 対象のヒエラルキーを変更します。UNDO可能。
- Asset の書き出しはしません。Unityを再起動すると、書き出していない Mesh などの Asset が消滅します。

(Prefab/Editor)
- 対象の prefab をシーンにコピーして処理を実行し、生成する Asset を指定されたフォルダに書き出します。
- Asset 書き出し後にコピーを削除します。
- Undo はありません。
")]
            [LangMsg(Languages.en, @"TODO
")]
            MESH_UTILITY,
        }
    }
}
