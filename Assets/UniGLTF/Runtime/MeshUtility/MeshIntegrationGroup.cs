using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public class MeshIntegrationGroup
    {
        /// <summary>
        /// FirstPerson flag
        /// TODO: enum
        /// </summary>
        public string Name;
        public List<Renderer> Renderers = new List<Renderer>();
    }
}