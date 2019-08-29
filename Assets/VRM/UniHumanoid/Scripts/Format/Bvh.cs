using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace UniHumanoid
{
    public class BvhException : Exception
    {
        public BvhException(string msg) : base(msg) { }
    }

    public enum Channel
    {
        Xposition,
        Yposition,
        Zposition,
        Xrotation,
        Yrotation,
        Zrotation,
    }
    public static class ChannelExtensions
    {
        public static string ToProperty(this Channel ch)
        {
            switch (ch)
            {
                case Channel.Xposition: return "localPosition.x";
                case Channel.Yposition: return "localPosition.y";
                case Channel.Zposition: return "localPosition.z";
                case Channel.Xrotation: return "localEulerAnglesBaked.x";
                case Channel.Yrotation: return "localEulerAnglesBaked.y";
                case Channel.Zrotation: return "localEulerAnglesBaked.z";
            }

            throw new BvhException("no property for " + ch);
        }

        public static bool IsLocation(this Channel ch)
        {
            switch (ch)
            {
                case Channel.Xposition:
                case Channel.Yposition:
                case Channel.Zposition: return true;
                case Channel.Xrotation: 
                case Channel.Yrotation: 
                case Channel.Zrotation: return false;
            }

            throw new BvhException("no property for " + ch);
        }
    }

    public struct Single3
    {
        public Single x;
        public Single y;
        public Single z;

        public Single3(Single _x, Single _y, Single _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    }

    public class BvhNode
    {
        public String Name
        {
            get;
            private set;
        }

        public Single3 Offset
        {
            get;
            private set;
        }

        public Channel[] Channels
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

        public virtual void Parse(StringReader r)
        {
            Offset = ParseOffset(r.ReadLine());

            Channels = ParseChannel(r.ReadLine());
        }

        static Single3 ParseOffset(string line)
        {
            var split = line.Trim().Split();
            if (split[0] != "OFFSET")
            {
                throw new BvhException("OFFSET is not found");
            }

            var offset = split.Skip(1).Where(x => !string.IsNullOrEmpty(x)).Select(x => float.Parse(x)).ToArray();
            return new Single3(offset[0], offset[1], offset[2]);
        }

        static Channel[] ParseChannel(string line)
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
            return split.Skip(2).Select(x => (Channel)Enum.Parse(typeof(Channel), x)).ToArray();
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

    public class EndSite : BvhNode
    {
        public EndSite(): base("")
        {
        }

        public override void Parse(StringReader r)
        {
            r.ReadLine(); // offset
        }
    }

    public class ChannelCurve
    {
        public float[] Keys
        {
            get;
            private set;
        }

        public ChannelCurve(int frameCount)
        {
            Keys = new float[frameCount];
        }

        public void SetKey(int frame, float value)
        {
            Keys[frame] = value;
        }
    }

    public class Bvh
    {
        public BvhNode Root
        {
            get;
            private set;
        }

        public TimeSpan FrameTime
        {
            get;
            private set;
        }

        public ChannelCurve[] Channels
        {
            get;
            private set;
        }

        int m_frames;
        public int FrameCount
        {
            get { return m_frames; }
        }

        public struct PathWithProperty
        {
            public string Path;
            public string Property;
            public bool IsLocation;
        }

        public bool TryGetPathWithPropertyFromChannel(ChannelCurve channel, out PathWithProperty pathWithProp)
        {
            var index = Channels.ToList().IndexOf(channel);
            if (index == -1)
            {
                pathWithProp = default(PathWithProperty);
                return false;
            }

            foreach(var node in Root.Traverse())
            {
                for(int i=0; i<node.Channels.Length; ++i, --index)
                {
                    if (index == 0)
                    {
                        pathWithProp = new PathWithProperty
                        {
                            Path=GetPath(node),
                            Property=node.Channels[i].ToProperty(),
                            IsLocation=node.Channels[i].IsLocation(),
                        };
                        return true;
                    }
                }
            }

            throw new BvhException("channel is not found");
        }

        public string GetPath(BvhNode node)
        {
            var list = new List<string>() { node.Name };

            var current = node;
            while (current!=null)
            {
                current = GetParent(current);
                if (current != null)
                {
                    list.Insert(0, current.Name);
                }
            }

            return String.Join("/", list.ToArray());
        }

        BvhNode GetParent(BvhNode node)
        {
            foreach(var x in Root.Traverse())
            {
                if (x.Children.Contains(node))
                {
                    return x;
                }
            }

            return null;
        }

        public ChannelCurve GetChannel(BvhNode target, Channel channel)
        {
            var index = 0;
            foreach (var node in Root.Traverse())
            {
                for (int i = 0; i < node.Channels.Length; ++i, ++index)
                {
                    if(node==target && node.Channels[i] == channel)
                    {
                        return Channels[index];
                    }
                }
            }

            throw new BvhException("channel is not found");
        }

        public override string ToString()
        {
            return string.Format("{0}nodes, {1}channels, {2}frames, {3:0.00}seconds"
                , Root.Traverse().Count()
                , Channels.Length
                , m_frames
                , m_frames * FrameTime.TotalSeconds);
        }

        public Bvh(BvhNode root, int frames, float seconds)
        {
            Root = root;
            FrameTime = TimeSpan.FromSeconds(seconds);
            m_frames = frames;
            var channelCount = Root.Traverse()
                .Where(x => x.Channels!=null)
                .Select(x => x.Channels.Length)
                .Sum();
            Channels = Enumerable.Range(0, channelCount)
                .Select(x => new ChannelCurve(frames))
                .ToArray()
                ;
        }

        public void ParseFrame(int frame, string line)
        {
            var split = line.Trim().Split().Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (split.Length != Channels.Length)
            {
                throw new BvhException("frame key count is not match channel count");
            }
            for(int i=0; i<Channels.Length; ++i)
            {
                Channels[i].SetKey(frame, float.Parse(split[i]));
            }
        }

        public static Bvh Parse(string src)
        {
            using (var r = new StringReader(src))
            {
                if (r.ReadLine() != "HIERARCHY")
                {
                    throw new BvhException("not start with HIERARCHY");
                }
               
                var root = ParseNode(r);
                if (root == null)
                {
                    return null;
                }

                var frames = 0;
                var frameTime = 0.0f;
                if (r.ReadLine() == "MOTION")
                {
                    var frameSplit = r.ReadLine().Split(':');
                    if (frameSplit[0] != "Frames")
                    {
                        throw new BvhException("Frames is not found");
                    }
                    frames = int.Parse(frameSplit[1]);

                    var frameTimeSplit = r.ReadLine().Split(':');
                    if (frameTimeSplit[0] != "Frame Time")
                    {
                        throw new BvhException("Frame Time is not found");
                    }
                    frameTime = float.Parse(frameTimeSplit[1]);
                }

                var bvh = new Bvh(root, frames, frameTime);

                for(int i=0; i<frames; ++i)
                {
                    var line = r.ReadLine();
                    bvh.ParseFrame(i, line);
                }
               
                return bvh;
            }
        }

        static BvhNode ParseNode(StringReader r, int level = 0)
        {
            var firstline = r.ReadLine().Trim();
            var split = firstline.Split();
            if (split.Length != 2)
            {
                if (split.Length == 1)
                {
                    if(split[0] == "}")
                    {
                        return null;
                    }
                }
                throw new BvhException(String.Format("split to {0}({1})", split.Length, firstline));
            }

            BvhNode node = null;
            if (split[0] == "ROOT")
            {
                if (level != 0)
                {
                    throw new BvhException("nested ROOT");
                }
                node = new BvhNode(split[1]);
            }
            else if (split[0] == "JOINT")
            {
                if (level == 0)
                {
                    throw new BvhException("should ROOT, but JOINT");
                }
                node = new BvhNode(split[1]);
            }
            else if (split[0] == "End")
            {
                if (level == 0)
                {
                    throw new BvhException("End in level 0");
                }
                node = new EndSite();
            }
            else
            {
                throw new BvhException("unknown type: " + split[0]);
            }

            if(r.ReadLine().Trim() != "{")
            {
                throw new BvhException("'{' is not found");
            }

            node.Parse(r);

            // child nodes
            while (true)
            {
                var child = ParseNode(r, level + 1);
                if (child == null)
                {
                    break;
                }

                if(!(child is EndSite))
                {
                    node.Children.Add(child);
                }
            }

            return node;
        }
    }
}
