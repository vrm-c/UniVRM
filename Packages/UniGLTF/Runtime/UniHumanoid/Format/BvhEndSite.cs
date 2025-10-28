using System.IO;

namespace UniHumanoid
{
    public class BvhEndSite : BvhNode
    {
        public BvhEndSite() : base("")
        {
        }

        public override void Parse(StringReader r)
        {
            r.ReadLine(); // offset
        }
    }
}
