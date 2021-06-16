using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10
{
    /// <summary>
    /// VRM関連の情報を集約したオブジェクト。
    /// 
    /// ScriptedImporter から Extract して Edit 可能な項目なるべく増やすべく導入。
    /// 
    /// * Meta(VRM必須)
    /// * Humanoid(VRM必須)
    /// * Expressions(enum + custom list)
    /// * LookAt
    /// * FirstPerson
    /// * SpringBone の MonoBehaviour でない部分
    ///   * ColliderGroup
    ///   * Springs
    /// 
    /// Serialize 可能な形で保持し、Editor経由で Edit可能にするのが目的。
    /// 
    /// </summary>
    public class VRM10Object : ScriptableObject
    {
        public static SubAssetKey SubAssetKey => new SubAssetKey(typeof(VRM10Object), "_vrm1_");

        [SerializeField]
        public VRM10ObjectMeta Meta = new VRM10ObjectMeta();

        [SerializeField]
        public VRM10ObjectExpression Expression = new VRM10ObjectExpression();

        [SerializeField]
        public VRM10ControllerLookAt LookAt = new VRM10ControllerLookAt();

        [SerializeField]
        public VRM10ObjectFirstPerson FirstPerson = new VRM10ObjectFirstPerson();

        [SerializeField]
        public VRM10ControllerSpringBone SpringBone = new VRM10ControllerSpringBone();

        void OnValidate()
        {
            Debug.Log($"VRM10Object.OnValidate");
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
