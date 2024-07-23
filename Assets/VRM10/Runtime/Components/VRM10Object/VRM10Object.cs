using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// VRM関連の情報を保持するオブジェクト
    /// ScriptedImporter から Extract して
    /// Editor経由で Edit可能にするのが目的。
    /// ヒエラルキーに対する参照を保持できないので Humanoid, Spring, Constraint は含まず
    /// 下記の項目を保持することとした。
    /// シーンに出さずにアセットとして編集できる。
    /// 
    /// * Meta
    /// * Expressions(enum + custom list)
    /// * LookAt
    /// * FirstPerson
    /// 
    /// </summary>
    public class VRM10Object : PrefabRelatedScriptableObject
    {
        public static SubAssetKey SubAssetKey => new SubAssetKey(typeof(VRM10Object), "_vrm1_");

        [SerializeField]
        public VRM10ObjectMeta Meta = new VRM10ObjectMeta();

        [SerializeField]
        public VRM10ObjectExpression Expression = new VRM10ObjectExpression();

        [SerializeField]
        public VRM10ObjectLookAt LookAt = new VRM10ObjectLookAt();

        [SerializeField]
        public VRM10ObjectFirstPerson FirstPerson = new VRM10ObjectFirstPerson();

        void OnValidate()
        {
            if (LookAt != null)
            {
                LookAt.HorizontalInner.OnValidate();
                LookAt.HorizontalOuter.OnValidate();
                LookAt.VerticalUp.OnValidate();
                LookAt.VerticalDown.OnValidate();
            }
        }
    }
}
