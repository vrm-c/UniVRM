using UnityEngine;

namespace UniGLTF
{
    internal class UniGLTFShaderNotMatchedInternalException : UniGLTFException
    {
        public UniGLTFShaderNotMatchedInternalException(Shader shader) : base(shader != null ? shader.name : "") { }
    }
}