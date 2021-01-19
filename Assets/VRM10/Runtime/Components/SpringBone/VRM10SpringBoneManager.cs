using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// すべての SpringBone を管理する。
    /// Humanoid 一体につきひとつだけ存在する。
    /// </summary>
    [Serializable]
    public class VRM10SpringBoneManager
    {
        [SerializeField]
        public List<VRM10SpringBone> Springs = new List<VRM10SpringBone>();

        /// <summary>
        /// 1フレームに一回呼び出す(VRM10Controllerの仕事)
        /// </summary>
        public void Process()
        {
            foreach (var spring in Springs)
            {
                spring.Process();
            }
        }
    }
}
