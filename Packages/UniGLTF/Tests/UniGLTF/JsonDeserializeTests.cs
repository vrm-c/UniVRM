using NUnit.Framework;
using UnityEngine;

namespace UniGLTF
{
    public class JsonDeserializeTests
    {
        static T deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}
