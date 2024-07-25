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
                UniGLTFLogger.Log($"{smr} has excluded");
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
            if (mr.TryGetComponent<MeshFilter>(out var filter))
            {
                return true;
            }
            if (filter.sharedMesh == null)
            {
                return true;
            }
            if (_excludes.Contains(filter.sharedMesh))
            {
                UniGLTFLogger.Log($"{mr} has excluded");
                return true;
            }
            return false;
        }
    }
}