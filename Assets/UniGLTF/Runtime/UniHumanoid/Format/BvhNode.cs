using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UniHumanoid
{
    public class BvhNode
    {
        public String Name
        {
            get;
            private set;
        }

        public Vector3 Offset
        {
            get;
            private set;
        }

        public BvhChannel[] Channels
        {
            get;
            private set;
        }

        public List<BvhNode> Children
        {
            get;
            private set;
        }

        public BvhNode(string name)
        {
            Name = name;
            Children = new List<BvhNode>();
        }

        public int GetChannelIndex(BvhChannel channel)
        {
            for (int i = 0; i < Channels.Length; ++i)
            {
                if (channel == Channels[i])
                {
                    return i;
                }
            }
            throw new KeyNotFoundException();
        }

        public virtual void Parse(StringReader r)
        {
            Offset = ParseOffset(r.ReadLine());

            Channels = ParseChannel(r.ReadLine());
        }

        private static Vector3 ParseOffset(string line)
        {
            string[] split = line.Trim().Split();
            if (split[0] != "OFFSET")
            {
                throw new BvhException("OFFSET is not found");
            }

            var offset = split.Skip(1).Where(x => !string.IsNullOrEmpty(x)).Select(x => float.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            return new Vector3(offset[0], offset[1], offset[2]);
        }

        private static BvhChannel[] ParseChannel(string line)
        {
            var split = line.Trim().Split();
            if (split[0] != "CHANNELS")
            {
                throw new BvhException("CHANNELS is not found");
            }
            var count = int.Parse(split[1]);
            if (count + 2 != split.Length)
            {
                throw new BvhException("channel count is not match with split count");
            }
            return split.Skip(2).Select(x => (BvhChannel)Enum.Parse(typeof(BvhChannel), x)).ToArray();
        }

        public IEnumerable<BvhNode> Traverse()
        {
            yield return this;

            foreach (var child in Children)
            {
                foreach (var descendant in child.Traverse())
                {
                    yield return descendant;
                }
            }
        }
    }
}
