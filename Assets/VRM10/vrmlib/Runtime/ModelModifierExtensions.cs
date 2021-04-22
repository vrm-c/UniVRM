using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace VrmLib
{
    public static class ModelModifierExtensions
    {
        public static string SkinningBake(this ModelModifier modifier)
        {
            foreach (var node in modifier.Model.Nodes)
            {
                var meshGroup = node.MeshGroup;
                if (meshGroup == null)
                {
                    continue;
                }

                if (meshGroup.Skin != null)
                {
                    // 正規化されていれば1つしかない
                    // されていないと Primitive の数だけある
                    foreach (var mesh in meshGroup.Meshes)
                    {
                        {
                            // Skinningの出力先を自身にすることでBakeする
                            meshGroup.Skin.Skinning(mesh.VertexBuffer);
                        }

                        //　morphのPositionは相対値が入っているはずなので、手を加えない（正規化されていない場合、二重に補正が掛かる）
                        /*
                                                foreach (var morph in mesh.MorphTargets)
                                                {
                                                    if (morph.VertexBuffer.Positions != null)
                                                    {
                                                        meshGroup.Skin.Skinning(morph.VertexBuffer);
                                                    }
                                                }
                                                */
                    }

                    meshGroup.Skin.Root = null;
                    meshGroup.Skin.InverseMatrices = null;
                }
                else
                {
                    foreach (var mesh in meshGroup.Meshes)
                    {
                        // nodeに対して疑似的にSkinningする
                        // 回転と拡縮を適用し位置は適用しない
                        mesh.ApplyRotationAndScaling(node.Matrix);
                    }
                }
            }

            // 回転・拡縮を除去する
            modifier.Model.ApplyRotationAndScale();

            // inverse matrix の再計算
            foreach (var node in modifier.Model.Nodes)
            {
                var meshGroup = node.MeshGroup;
                if (meshGroup == null)
                {
                    continue;
                }

                foreach (var mesh in meshGroup.Meshes)
                {
                    if (meshGroup.Skin != null)
                    {
                        meshGroup.Skin.CalcInverseMatrices();
                    }
                }
            }

            return "SkinningBake";
        }
    }
}
