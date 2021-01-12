namespace VrmLib
{
    public class MorphTargetBind
    {
        /// <summary>
        /// 対象のMesh(Renderer)
        /// </summary>
        public Node Node;

        /// <summary>
        /// MorphTarget の name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// MorphTarget の適用度
        /// </summary>
        public readonly float Value;

        public MorphTargetBind(Node node, string name, float value)
        {
            Node = node;
            Name = name;
            Value = value;
        }
    }
}
