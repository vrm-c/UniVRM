namespace VrmLib
{
    public class MorphTarget
    {
        public readonly string Name;
        public VertexBuffer VertexBuffer;

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(Name);
            foreach (var kv in VertexBuffer)
            {
                sb.Append($"[{kv.Key}]");
            }
            return sb.ToString();
        }

        public MorphTarget(string name)
        {
            Name = name;
        }
    }
}