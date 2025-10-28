using UniGLTF.M17N;

namespace UniGLTF.MeshUtility
{
    public enum MeshUtilityMessages
    {
        [LangMsg(Languages.ja, "ターゲットオブジェクト")]
        [LangMsg(Languages.en, "TargetObject")]
        TARGET_OBJECT,

        [LangMsg(Languages.ja, @"凍結 > 統合 > 分割 という一連の処理を実行します。

[凍結]
- ヒエラルキーの 回転・拡縮を Mesh に焼き付けます。
- BlendShape の現状を Mesh に焼き付けます。

[統合]
- ヒエラルキーに含まれる MeshRenderer と SkinnedMeshRenderer をひとつの SkinnedMeshRenderer に統合します。

[分割]
- 統合結果を BlendShape の有無を基準に分割します。

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
        [LangMsg(Languages.en, @"
Separate the mesh attached to the SkinnedMeshRenderer under the target object with or without BlendShape.

* Asset: A new Mesh Asset will be created in the same folder as the original. Example: Original-> Original_WithBlendShape.mesh & Original_WithoutBlendShape.mesh
* Scene: In the copied hierarchy, the split Mesh is replaced with a Mesh that holds the BlendShape, and a SkinnedMeshRenderer with a Mesh without BlendShape is added.
")]
        MESH_UTILITY,

        [LangMsg(Languages.ja, "ブレンドシェイプの有無で分割する")]
        [LangMsg(Languages.en, "Divide by the presence or absence of `blendshape`")]
        MESH_SEPARATOR_BY_BLENDSHAPE,

        //         [LangMsg(Languages.ja, @"ターゲットオブジェクト下の SkinnedMeshRenderer または MeshFilter にアタッチされたメッシュを統合します。

        // * Asset: Assets/MeshIntegrated.mesh が作成されます(上書きされるので注意してください)。
        // * Scene: コピーされたヒエラルキーでは、統合された Mesh は除去されます。新しく MeshIntegrator ノードが追加されます。
        // * VRMではBlendShapeClipの統合など追加の処理が必要です。`VRM0-MeshIntegratorWizard` を使ってください。
        // ")]
        //         [LangMsg(Languages.en, @"Integrates the attached mesh into the SkinnedMeshRenderer or MeshFilter under the target object.

        // * Asset: Assets/MeshIntegrated.mesh is created (note that it will be overwritten).
        // * Scene: In the copied hierarchy, the integrated mesh is removed. A new MeshIntegrator node is added.
        // * VRM requires additional processing such as BlendShapeClip integration. Use the `VRM0-MeshIntegratorWizard` integration feature.
        // ")]
        //         MESH_INTEGRATOR,

        //         // [LangMsg(Languages.ja, "静的メッシュを一つに統合します")]
        //         // [LangMsg(Languages.en, "Integrate static meshes into one")]
        //         // STATIC_MESH_INTEGRATOR,

        //         [LangMsg(Languages.ja, @"指定された SkinnedMeshRenderer から、指定されたボーンに対する Weight を保持する三角形を除去します。

        // * Asset: 元の Mesh と同じフォルダに、三角形を除去した Mesh を保存します。
        // * Scene: コピーされたヒエラルキーでは、三角形が除去された Mesh に差し替えられます。
        // ")]
        //         [LangMsg(Languages.en, @"Removes the triangle that holds the weight for the specified bone from the specified SkinnedMeshRenderer.

        // * Assets: Save the mesh with the triangles removed in the same folder as the original mesh.
        // * Scene: In the copied hierarchy, it will be replaced with a Mesh with the triangles removed.
        // ")]

        //         BONE_MESH_ERASER,

        [LangMsg(Languages.ja, "Skinned Meshを選んでください")]
        [LangMsg(Languages.en, "Select a skinned mesh")]
        SELECT_SKINNED_MESH,

        // [LangMsg(Languages.ja, "Erase Rootを選んでください")]
        // [LangMsg(Languages.en, "Select a erase root")]
        // SELECT_ERASE_ROOT,

        [LangMsg(Languages.ja, "GameObjectを選んでください")]
        [LangMsg(Languages.en, "Select a GameObject first")]
        NO_GAMEOBJECT_SELECTED,

        [LangMsg(Languages.ja, "GameObjectにスキンメッシュが含まれていません")]
        [LangMsg(Languages.en, "No skinned mesh is contained")]
        NO_SKINNED_MESH,

        // [LangMsg(Languages.ja, "GameObjectに静的メッシュが含まれていません")]
        // [LangMsg(Languages.en, "No static mesh is contained")]
        // NO_STATIC_MESH,

        [LangMsg(Languages.ja, "GameObjectにスキンメッシュ・静的メッシュが含まれていません")]
        [LangMsg(Languages.en, "Skinned/Static mesh is not contained")]
        NO_MESH,

        [LangMsg(Languages.ja, "BlendShapeClipが不整合を起こすので、`VRM0-> MeshIntegrator`を使ってください")]
        [LangMsg(Languages.en, "Because BlendShapeClip causes inconsistency , use `VRM0 -> MeshIntegrator` instead")]
        VRM_DETECTED,

        [LangMsg(Languages.ja, "対象は, Prefab Asset です。実行時に書き出しファイルの指定があります。")]
        [LangMsg(Languages.en, "The target is prefab asset. A temporary file is specified during execution.")]
        PREFAB_ASSET,

        [LangMsg(Languages.ja, "対象は, Prefab Instance です。Unpack されます。")]
        [LangMsg(Languages.en, "The target is prefab asset. A temporary file is specified during execution.")]
        PREFAB_INSTANCE,
    }
}