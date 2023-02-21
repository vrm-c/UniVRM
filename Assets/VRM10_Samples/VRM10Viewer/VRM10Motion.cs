using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using UniHumanoid;
using UniJSON;
using UnityEngine;
using VRMShaders;

namespace UniVRM10.VRM10Viewer
{
    public class VRM10Motion
    {
        public (INormalizedPoseProvider, ITPoseProvider) ControlRig;

        UniHumanoid.BvhImporterContext m_context;
        UniGLTF.RuntimeGltfInstance m_instance;

        public Transform Root => m_context?.Root.transform;

        public VRM10Motion(UniHumanoid.BvhImporterContext context)
        {
            m_context = context;
            var provider = new AnimatorPoseProvider(m_context.Root.transform, m_context.Root.GetComponent<Animator>());
            ControlRig = (provider, provider);
        }

        public VRM10Motion(UniGLTF.RuntimeGltfInstance instance)
        {
            m_instance = instance;
            if (instance.GetComponent<Animation>() is Animation animation)
            {
                animation.Play();
            }
        }

        public void ShowBoxMan(bool showBoxMan)
        {
            if (m_context != null)
            {
                m_context.Root.GetComponent<SkinnedMeshRenderer>().enabled = showBoxMan;
            }
        }

        public static VRM10Motion LoadBvhFromText(string source, string path = "tmp.bvh")
        {
            var context = new UniHumanoid.BvhImporterContext();
            context.Parse(path, source);
            context.Load();
            return new VRM10Motion(context);
        }

        public static VRM10Motion LoadBvhFromPath(string path)
        {
            return LoadBvhFromText(File.ReadAllText(path), path);
        }

        static IEnumerable<Transform> Traverse(Transform t)
        {
            yield return t;
            foreach (Transform child in t)
            {
                foreach (var x in Traverse(child))
                {
                    yield return x;
                }
            }
        }
    }
}
