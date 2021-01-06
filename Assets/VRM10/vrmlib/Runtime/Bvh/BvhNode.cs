using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace VrmLib.Bvh
{
    public class BvhNode
    {
        public String Name
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public HumanoidBones Bone
        {
            get;
            set;
        }

        // world position
        public Vector3 SkeletonLocalPosition
        {
            get;
            private set;
        }

        public void UpdatePosition(Vector3 parentPosition)
        {
            SkeletonLocalPosition = parentPosition + Offset;

            foreach (var child in Children)
            {
                child.UpdatePosition(SkeletonLocalPosition);
            }
        }

        public Vector3 Offset;

        public Channel[] Channels
        {
            get;
            private set;
        }

        List<BvhNode> m_children = new List<BvhNode>();


        public IReadOnlyList<BvhNode> Children => m_children;


        public void AddChid(BvhNode child)
        {
            child.Parent = this;
            m_children.Add(child);
        }


        public BvhNode Parent
        {
            get;
            private set;
        }

        public BvhNode(string name)
        {
            Name = name;
        }

        public virtual void Parse(StringReader r)
        {
            Offset = ParseOffset(r.ReadLine());

            Channels = ParseChannel(r.ReadLine());
        }

        static Vector3 ParseOffset(string line)
        {
            var splited = line.Trim().Split();
            if (splited[0] != "OFFSET")
            {
                throw new BvhException("OFFSET is not found");
            }

            var offset = splited.Skip(1).Where(x => !string.IsNullOrEmpty(x)).Select(x => float.Parse(x)).ToArray();
            return new Vector3(offset[0], offset[1], offset[2]);
        }

        static Channel[] ParseChannel(string line)
        {
            var splited = line.Trim().Split();
            if (splited[0] != "CHANNELS")
            {
                throw new BvhException("CHANNELS is not found");
            }
            var count = int.Parse(splited[1]);
            if (count + 2 != splited.Length)
            {
                throw new BvhException("channel count is not match with splited count");
            }
            return splited.Skip(2).Select(x => (Channel)Enum.Parse(typeof(Channel), x)).ToArray();
        }

        public IEnumerable<BvhNode> Traverse()
        {
            yield return this;

            foreach (var child in Children)
            {
                foreach (var descentant in child.Traverse())
                {
                    yield return descentant;
                }
            }
        }
    }
}