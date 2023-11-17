using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniGLTF.MeshUtility
{
    public class MeshIntegrationGroup
    {
        public string Name;
        public List<Renderer> Renderers = new List<Renderer>();

        public static List<MeshIntegrationGroup> ToList()
        {
            throw new NotImplementedException();
        }
    }
}