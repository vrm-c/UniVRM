using UnityEngine;

namespace UniVRM10
{
    [CreateAssetMenu(menuName = "VRM10/Expression")]
    public sealed class VRM10Expression : PrefabRelatedScriptableObject
    {
        /// <summary>
        /// 対象メッシュの Expression を操作する
        /// <summary>
        [SerializeField]
        public MorphTargetBinding[] MorphTargetBindings = new MorphTargetBinding[] { };

        /// <summary>
        /// 対象マテリアルの Color を操作する
        /// <summary>
        [SerializeField]
        public MaterialColorBinding[] MaterialColorBindings = new MaterialColorBinding[] { };

        /// <summary>
        /// 対象マテリアルの UVScale+Offset を操作する
        /// <summary>
        [SerializeField]
        public MaterialUVBinding[] MaterialUVBindings = new MaterialUVBinding[] { };

        /// <summary>
        /// UniVRM-0.45: trueの場合、この Expression は0と1の間の中間値を取らない。四捨五入する
        /// </summary>
        [SerializeField]
        public bool IsBinary;

        /// <summary>
        /// この Expression と Blink(Blink, BlinkLeft, BlinkRight) が同時に有効な場合、Blink の Weight を 0 にする
        /// </summary>
        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType OverrideBlink;

        /// <summary>
        /// この Expression と LookAt(LookUp, LookDown, LookLeft, LookRight) が同時に有効な場合、LookAt の Weight を 0 にする
        /// </summary>
        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType OverrideLookAt;

        /// <summary>
        /// この Expression と Mouth(Aa, Ih, Ou, Ee, Oh) が同時に有効な場合、Mouth の Weight を 0 にする
        /// </summary>
        [SerializeField]
        public UniGLTF.Extensions.VRMC_vrm.ExpressionOverrideType OverrideMouth;
    }
}
