using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public class MeshExclude
    {
        List<Mesh> _excludes = new List<Mesh>();

        public MeshExclude(IEnumerable<Mesh> excludes)
        {
            if (excludes != null)
            {
                _excludes.AddRange(excludes);
            }
        }

        public bool IsExcluded(SkinnedMeshRenderer smr)
        {
            if (smr == null)
            {
                return true;
            }
            if (smr.sharedMesh == null)
            {
                return true;
            }
            if (_excludes.Contains(smr.sharedMesh))
            {
                Debug.LogFormat("{0} has excluded", smr);
                return true;
            }
            return false;
        }

        public bool IsExcluded(MeshRenderer mr)
        {
            if (mr == null)
            {
                return true;
            }
            var filter = mr.GetComponent<MeshFilter>();
            if (filter == null)
            {
                return true;
            }
            if (filter.sharedMesh == null)
            {
                return true;
            }
            if (_excludes.Contains(filter.sharedMesh))
            {
                Debug.LogFormat("{0} has excluded", mr);
                return true;
            }
            return false;
        }
    }
}
