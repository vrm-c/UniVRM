using System;
using System.Collections.Generic;
using System.Numerics;

namespace VrmLib
{
    public static class VertexBufferExtensions
    {
        public static void Add<T>(this VertexBuffer v, string key, T[] list) where T : struct
        {
            v.Add(key, BufferAccessor.Create(list));
        }

        public static void FixBoneWeight(this VertexBuffer v)
        {
            var joints = v.Joints.GetSpan<SkinJoints>();
            var weights = v.Weights.GetSpan<Vector4>();
            if (joints.Length != weights.Length)
            {
                throw new System.Exception();
            }

            for (int i = 0; i < joints.Length; ++i)
            {
                var j = joints[i];
                var w = weights[i];

                int n = 0;
                if (w.X > 0) ++n;
                if (w.Y > 0) ++n;
                if (w.Z > 0) ++n;
                if (w.W > 0) ++n;
                if (n == 1)
                {
                    if (w.X == 0)
                    {
                        if (w.Y > 0)
                        {
                            w.X = w.Y;
                            w.Y = 0;
                            j.Joint0 = j.Joint1;
                            j.Joint1 = 0;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
                else if (n == 2)
                {
                    if (w.X == 0 || w.Y == 0)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (w.X >= w.Y)
                        {

                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
                else if (n == 3)
                {
                    if (w.W != 0)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (w.X >= w.Y && w.Y >= w.Z)
                        {

                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
                else if (n == 4)
                {
                    if (w.X >= w.Y && w.Y >= w.Z && w.Z >= w.W)
                    {

                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                joints[i] = j;
                weights[i] = w;
            }
        }
    }
}