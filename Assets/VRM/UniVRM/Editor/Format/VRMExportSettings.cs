using System;
using UnityEngine;

namespace VRM
{
    [Serializable]
    public class VRMExportSettings : ScriptableObject
    {
        /// <summary>
        /// エクスポート時に強制的にT-Pose化する
        /// </summary>
        [Tooltip("Option")]
        public bool ForceTPose = false;

        /// <summary>
        /// エクスポート時にヒエラルキーの正規化を実施する
        /// </summary>
        [Tooltip("Require only first time")]
        public bool PoseFreeze = true;

        /// <summary>
        /// エクスポート時に新しいJsonSerializerを使う
        /// </summary>
        [Tooltip("Use new JSON serializer")]
        public bool UseExperimentalExporter = false;

        /// <summary>
        /// BlendShapeのシリアライズにSparseAccessorを使う
        /// </summary>
        [Tooltip("Use sparse accessor for blendshape. This may reduce vrm size")]
        public bool UseSparseAccessor = false;

        /// <summary>
        /// BlendShapeのPositionのみをエクスポートする
        /// </summary>
        [Tooltip("UniVRM-0.54 or later can load it. Otherwise fail to load")]
        public bool OnlyBlendshapePosition = false;

        /// <summary>
        /// エクスポート時にBlendShapeClipから参照されないBlendShapeを削除する
        /// </summary>
        [Tooltip("Remove blendshape that is not used from BlendShapeClip")]
        public bool ReduceBlendshape = false;

        /// <summary>
        /// skip if BlendShapeClip.Preset == Unknown
        /// </summary>
        [Tooltip("Remove blendShapeClip that preset is Unknown")]
        public bool ReduceBlendshapeClip = false;

        /// <summary>
        /// 頂点カラーを削除する
        /// </summary>
        [Tooltip("Remove vertex color")]
        public bool RemoveVertexColor = false;
   }
}
