using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF
{
    public interface IAnimationExporter
    {
        void Export(ExportingGltfData _data, GameObject Copy, List<Transform> Nodes);
    }
}
