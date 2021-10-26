using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UniJSON;

namespace UniGLTF
{
    public static class glTFExtensions
    {
        struct ComponentVec
        {
            public glComponentType ComponentType;
            public int ElementCount;

            public ComponentVec(glComponentType componentType, int elementCount)
            {
                ComponentType = componentType;
                ElementCount = elementCount;
            }
        }

        static Dictionary<Type, ComponentVec> ComponentTypeMap = new Dictionary<Type, ComponentVec>
        {
            { typeof(Vector2), new ComponentVec(glComponentType.FLOAT, 2) },
            { typeof(Vector3), new ComponentVec(glComponentType.FLOAT, 3) },
            { typeof(Vector4), new ComponentVec(glComponentType.FLOAT, 4) },
            { typeof(UShort4), new ComponentVec(glComponentType.UNSIGNED_SHORT, 4) },
            { typeof(Matrix4x4), new ComponentVec(glComponentType.FLOAT, 16) },
            { typeof(Color), new ComponentVec(glComponentType.FLOAT, 4) },
        };

        public static glComponentType GetComponentType<T>()
        {
            var cv = default(ComponentVec);
            if (ComponentTypeMap.TryGetValue(typeof(T), out cv))
            {
                return cv.ComponentType;
            }
            else if (typeof(T) == typeof(sbyte))
            {
                return glComponentType.BYTE;
            }
            else if (typeof(T) == typeof(byte))
            {
                return glComponentType.UNSIGNED_BYTE;
            }
            else if (typeof(T) == typeof(ushort))
            {
                return glComponentType.UNSIGNED_SHORT;
            }
            else if (typeof(T) == typeof(uint))
            {
                return glComponentType.UNSIGNED_INT;
            }
            else if (typeof(T) == typeof(float))
            {
                return glComponentType.FLOAT;
            }
            else
            {
                throw new NotImplementedException(typeof(T).Name);
            }
        }

        public static string GetAccessorType<T>()
        {
            var cv = default(ComponentVec);
            if (ComponentTypeMap.TryGetValue(typeof(T), out cv))
            {
                switch (cv.ElementCount)
                {
                    case 2: return "VEC2";
                    case 3: return "VEC3";
                    case 4: return "VEC4";
                    case 16: return "MAT4";
                    default: throw new Exception();
                }
            }
            else
            {
                return "SCALAR";
            }
        }

        static int GetAccessorElementCount<T>()
        {
            var cv = default(ComponentVec);
            if (ComponentTypeMap.TryGetValue(typeof(T), out cv))
            {
                return cv.ElementCount;
            }
            else
            {
                return 1;
            }
        }

        public static bool IsGeneratedUniGLTFAndOlderThan(string generatorVersion, int major, int minor)
        {
            if (string.IsNullOrEmpty(generatorVersion)) return false;
            if (generatorVersion == "UniGLTF") return true;
            if (!generatorVersion.FastStartsWith("UniGLTF-")) return false;

            try
            {
                var splitted = generatorVersion.Substring(8).Split('.');
                var generatorMajor = int.Parse(splitted[0]);
                var generatorMinor = int.Parse(splitted[1]);

                if (generatorMajor < major)
                {
                    return true;
                }
                else if (generatorMajor > major)
                {
                    return false;
                }
                else
                {
                    if (generatorMinor >= minor)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("{0}: {1}", generatorVersion, ex);
                return false;
            }
        }

        public static bool IsGeneratedUniGLTFAndOlder(this glTF gltf, int major, int minor)
        {
            if (gltf == null) return false;
            if (gltf.asset == null) return false;
            return IsGeneratedUniGLTFAndOlderThan(gltf.asset.generator, major, minor);
        }
    }
}
