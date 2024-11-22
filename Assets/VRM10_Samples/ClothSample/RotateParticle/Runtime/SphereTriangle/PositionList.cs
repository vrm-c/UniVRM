using System;
using System.Collections.Generic;
using UnityEngine;

namespace SphereTriangle
{
    public delegate void InitPosition(int index, float mass, in Vector3 position);

    public class PositionList
    {
        // 初期状態
        // 質点の重さ(0は移動しない)
        float[] Mass;

        // 衝突前
        Vector3[] Positions;
        public int[] CollisionCount;

        // 衝突による移動距離
        public Vector3[] Delta;
        // Positions に Delta を反映した結果
        public Vector3[] Result;

        public PositionList(int count)
        {
            Positions = new Vector3[count];
            CollisionCount = new int[count];
            Mass = new float[count];
            Delta = new Vector3[count];
            Result = new Vector3[count];
        }

        public Dictionary<ClothRect, Bounds> BoundsCache = new();

        public static Bounds GetBoundsFrom4(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d)
        {
            var aabb = new Bounds(a, Vector3.zero);
            aabb.Encapsulate(b);
            aabb.Encapsulate(c);
            aabb.Encapsulate(d);
            return aabb;
        }

        public Bounds GetBounds(ClothRect rect)
        {
            if (BoundsCache.TryGetValue(rect, out var b))
            {
                return b;
            }
            else
            {
                b = GetBoundsFrom4(Get(rect._a), Get(rect._b), Get(rect._c), Get(rect._d));
                BoundsCache.Add(rect, b);
                return b;
            }
        }

        public Vector3 Get(int index)
        {
            return Positions[index];
        }

        public void Init(int index, float mass, in Vector3 pos)
        {
            Mass[index] = mass;
            Positions[index] = pos;
            CollisionCount[index] = 0;
            Delta[index] = Vector3.zero;
        }

        public void EndInitialize()
        {
            // Buffer.BlockCopy(Positions, 0, Result, 0, Positions.Length);
            for (int i = 0; i < Positions.Length; ++i)
            {
                Result[i] = Positions[i];
            }
        }

        /// <summary>
        /// 衝突した。移動を蓄積
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        public void CollisionMove(int index, in LineSegment l, float radius, float factor = 1.0f)
        {
            // using var profile = new ProfileSample("CollisionMove");
            if (Mass[index] > 0)
            {
                Delta[index] += l.GetDelta(radius) * factor;
                ++CollisionCount[index];
            }
        }

        public IReadOnlyList<Vector3> Resolve()
        {
            for (int i = 0; i < Positions.Length; ++i)
            {
                if (CollisionCount[i] > 0)
                {
                    Result[i] = Positions[i] + Delta[i] / CollisionCount[i];
                }
                else
                {
                    Result[i] = Positions[i];
                }
            }
            return Result;
        }

        /// <summary>
        /// 衝突前後の状態を描画
        /// </summary>
        public void DrawGizmos()
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < Positions.Length; ++i)
            {
                if (CollisionCount[i] > 0)
                {
                    Gizmos.DrawLine(Positions[i], Result[i]);
                    // Gizmos.DrawWireSphere(Positions[i], 0.01f);
                    // Gizmos.DrawSphere(Result[i], 0.01f);
                }
            }
        }
    }
}