using System.Collections.Generic;
using UnityEngine;


namespace UniHumanoid
{
    /// <summary>
    /// Mapping HumanBodyBones to BoneIndex
    /// </summary>
    public struct Skeleton
    {
        Dictionary<HumanBodyBones, int> m_indexMap;
        public Dictionary<HumanBodyBones, int> Bones
        {
            get { return m_indexMap; }
        }
        public int GetBoneIndex(HumanBodyBones bone)
        {
            int index;
            if (m_indexMap.TryGetValue(bone, out index))
            {
                return index;
            }
            else
            {
                return -1;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// For UnitTest
        /// </summary>
        Dictionary<HumanBodyBones, string> m_nameMap;
        /// <summary>
        /// ForTest
        /// </summary>
        /// <param name="bone"></param>
        /// <returns></returns>
        public string GetBoneName(HumanBodyBones bone)
        {
            string name;
            if (m_nameMap.TryGetValue(bone, out name))
            {
                return name;
            }
            else
            {
                return null;
            }
        }
#endif

        public static Skeleton Estimate(Transform hips)
        {
            var estimater = new BvhSkeletonEstimator();
            return estimater.Detect(hips);
        }

        /// <summary>
        /// Register bone's HumanBodyBones
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="bones"></param>
        /// <param name="b"></param>
        public void Set(HumanBodyBones bone, IList<IBone> bones, IBone b)
        {
            if (b != null)
            {
                Set(bone, bones.IndexOf(b), b.Name);
            }
        }

        public void Set(HumanBodyBones bone, int index, string name)
        {
            if (m_indexMap == null)
            {
                m_indexMap = new Dictionary<HumanBodyBones, int>();
            }
            m_indexMap[bone] = index;

#if UNITY_EDITOR
            if (m_nameMap == null)
            {
                m_nameMap = new Dictionary<HumanBodyBones, string>();
            }
            m_nameMap[bone] = name;
#endif
        }

    }
}
