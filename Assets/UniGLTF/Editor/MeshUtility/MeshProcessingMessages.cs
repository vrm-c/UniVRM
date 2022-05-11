using UniGLTF.M17N;

namespace UniGLTF.MeshUtility
{
    public enum MeshProcessingMessages
    {
        [LangMsg(Languages.ja, "ターゲットオブジェクト")]
        [LangMsg(Languages.en, "TargetObject")]
        TARGET_OBJECT,

        [LangMsg(Languages.ja, @"ターゲットオブジェクト下の SkinnedMeshRenderer にアタッチされたメッシュを、 BlendShape の有無で分割します。

* Asset: 新しい Mesh Asset が元と同じフォルダに作成されます。例: Original -> Original_WithBlendShape.mesh & Original_WithoutBlendShape.mesh
* Scene: コピーされたヒエラルキーでは、分割された Mesh は BlendShape のある Mesh で置き換えられて、BlendShape の無い Mesh を使った SkinnedMeshRenderer が追加されます。
")]
        [LangMsg(Languages.en, @"Separate the mesh attached to the SkinnedMeshRenderer under the target object with or without BlendShape.

* Asset: A new Mesh Asset will be created in the same folder as the original. Example: Original-> Original_WithBlendShape.mesh & Original_WithoutBlendShape.mesh
* Scene: In the copied hierarchy, the split Mesh is replaced with a Mesh that holds the BlendShape, and a SkinnedMeshRenderer with a Mesh without BlendShape is added.
")]
        MESH_SEPARATOR,

        [LangMsg(Languages.ja, @"ターゲットオブジェクト下の SkinnedMeshRenderer または MeshFilter にアタッチされたメッシュを統合します。BlendShape の有無で２つ作成されます。

* Asset: Assets/MeshIntegrated.mesh が作成されます(上書きされるので注意してください)。
* Scene: コピーされたヒエラルキーでは、統合された Mesh は除去されます。新しく MeshIntegrator ノードが追加されます。
* VRMではBlendShapeClipの統合など追加の処理が必要でる。VRMの統合機能を使ってください。
")]
        [LangMsg(Languages.en, @"Integrates the attached mesh into the SkinnedMeshRenderer or MeshFilter under the target object. A mesh that holds BlendShape and a mesh that does not hold BlendShape are created.

* Asset: Assets/MeshIntegrated.mesh is created (note that it will be overwritten).
* Scene: In the copied hierarchy, the integrated mesh is removed. A new MeshIntegrator node is added.
* VRM requires additional processing such as BlendShapeClip integration. Use the VRM integration feature.
")]
        MESH_INTEGRATOR,

        // [LangMsg(Languages.ja, "静的メッシュを一つに統合します")]
        // [LangMsg(Languages.en, "Integrate static meshes into one")]
        // STATIC_MESH_INTEGRATOR,

        [LangMsg(Languages.ja, @"指定された SkinnedMeshRenderer から、指定されたボーンに対する Weight を保持する三角形を除去します。

* Asset: 元の Mesh と同じフォルダに、三角形を除去した Mesh を保存します。
* Scene: コピーされたヒエラルキーでは、三角形が除去された Mesh に差し替えられます。
")]
        [LangMsg(Languages.en, @"Removes the triangle that holds the weight for the specified bone from the specified SkinnedMeshRenderer.

* Assets: Save the mesh with the triangles removed in the same folder as the original mesh.
* Scene: In the copied hierarchy, it will be replaced with a Mesh with the triangles removed.
")]

        BONE_MESH_ERASER,

        [LangMsg(Languages.ja, "Skinned Meshを選んでください")]
        [LangMsg(Languages.en, "Select a skinned mesh")]
        SELECT_SKINNED_MESH,

        [LangMsg(Languages.ja, "Erase Rootを選んでください")]
        [LangMsg(Languages.en, "Select a erase root")]
        SELECT_ERASE_ROOT,

        [LangMsg(Languages.ja, "GameObjectを選んでください")]
        [LangMsg(Languages.en, "Select a GameObject first")]
        NO_GAMEOBJECT_SELECTED,

        [LangMsg(Languages.ja, "GameObjectにスキンメッシュが含まれていません")]
        [LangMsg(Languages.en, "No skinned mesh is contained")]
        NO_SKINNED_MESH,

        [LangMsg(Languages.ja, "GameObjectに静的メッシュが含まれていません")]
        [LangMsg(Languages.en, "No static mesh is contained")]
        NO_STATIC_MESH,

        [LangMsg(Languages.ja, "GameObjectにスキンメッシュ・静的メッシュが含まれていません")]
        [LangMsg(Languages.en, "Skinned/Static mesh is not contained")]
        NO_MESH,

        [LangMsg(Languages.ja, "ターゲットオブジェクトはVRMモデルです。`VRM0-> MeshIntegrator`を使ってください")]
        [LangMsg(Languages.en, "Target object is VRM model, use `VRM0 -> MeshIntegrator` instead")]
        VRM_DETECTED,
    }
}