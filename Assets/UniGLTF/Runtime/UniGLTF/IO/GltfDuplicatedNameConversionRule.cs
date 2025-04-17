using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UniGLTF
{
    /// <summary>
    /// `v0.129.0`
    /// glTF は仕様として、node 等の名前の重複を許している。 
    /// しかし Unity にロードする上では、名前が重複すると困る場面が多い。 
    /// したがって UniVRM ではロード時に、名前を重複しないように破壊的変更を行う仕様とする。 
    /// ここではその命名変更規則を定義する。
    /// </summary>
    public static class GltfDuplicatedNameConversionRule
    {
        public static readonly string UniqueFixResourceSuffix = "__UNIGLTF__DUPLICATED__";
        private static readonly Regex _removeUniqueFixResourceSuffix = new Regex($@"^(.+){UniqueFixResourceSuffix}(\d+)$");

        public static string FixNameUnique(HashSet<string> used, string originalName)
        {
            if (used.Add(originalName))
            {
                return originalName;
            }

            var duplicatedIdx = 2;
            while (true)
            {
                var newName = $"{originalName}{UniqueFixResourceSuffix}{duplicatedIdx++}";
                if (used.Add(newName))
                {
                    return newName;
                }
            }
        }

        /// <summary>
        /// GlbLowLevelParser.FixNameUnique で付与した Suffix を remove
        /// </summary>
        public static void FixName(glTF gltf)
        {
            var regex = new Regex($@"{UniqueFixResourceSuffix}\d+$");
            foreach (var gltfImages in gltf.images)
            {
                if (regex.IsMatch(gltfImages.name))
                {
                    gltfImages.name = regex.Replace(gltfImages.name, string.Empty);
                }
            }
            foreach (var gltfMaterial in gltf.materials)
            {
                if (regex.IsMatch(gltfMaterial.name))
                {
                    gltfMaterial.name = regex.Replace(gltfMaterial.name, string.Empty);
                }
            }
            foreach (var gltfAnimation in gltf.animations)
            {
                if (regex.IsMatch(gltfAnimation.name))
                {
                    gltfAnimation.name = regex.Replace(gltfAnimation.name, string.Empty);
                }
            }
        }

        public static bool TryGetOriginalName(string name, out string originalName)
        {
            var match = _removeUniqueFixResourceSuffix.Match(name);
            if (match.Success)
            {
                originalName = match.Groups[1].Value;
                return true;
            }
            else
            {
                originalName = name;
                return false;
            }
        }

        public static void FixMeshNameUnique(glTF GLTF)
        {
            var used = new HashSet<string>();
            foreach (var mesh in GLTF.meshes)
            {
                if (string.IsNullOrEmpty(mesh.name))
                {
                    // empty
                    mesh.name = "mesh_" + Guid.NewGuid().ToString("N");
                    used.Add(mesh.name);
                }
                else
                {
                    var lower = mesh.name.ToLower();
                    if (used.Contains(lower))
                    {
                        // rename
                        var uname = lower + "_" + Guid.NewGuid().ToString("N");
                        mesh.name = uname;
                        lower = uname;
                    }
                    used.Add(lower);
                }
            }
        }

        public static void RenameImageFromTexture(glTF GLTF, int i)
        {
            foreach (var texture in GLTF.textures)
            {
                if (texture.source == i)
                {
                    if (!string.IsNullOrEmpty(texture.name))
                    {
                        GLTF.images[i].name = texture.name;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// gltfTexture.name を Unity Asset 名として運用する。
        /// ユニークである必要がある。
        /// </summary>
        public static void FixTextureNameUnique(glTF GLTF)
        {
            // NOTE: Windows FileSystem は大文字小文字の違いは同名ファイルとして扱ってしまうため, IgnoreCase で評価する.
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var textureIdx = 0; textureIdx < GLTF.textures.Count; ++textureIdx)
            {
                var gltfTexture = GLTF.textures[textureIdx];
                if (gltfTexture.source.HasValidIndex())
                {
                    var gltfImage = GLTF.images[gltfTexture.source.Value];
                    if (!string.IsNullOrEmpty(gltfImage.uri) && !gltfImage.uri.StartsWith("data:"))
                    {
                        // from image uri
                        gltfTexture.name = Path.GetFileNameWithoutExtension(gltfImage.uri);
                    }
                    if (string.IsNullOrEmpty(gltfTexture.name))
                    {
                        // use image name
                        gltfTexture.name = gltfImage.name;
                    }
                }
                if (string.IsNullOrEmpty(gltfTexture.name))
                {
                    gltfTexture.name = $"texture_{textureIdx}";
                }

                gltfTexture.name = FixNameUnique(used, gltfTexture.name);
            }
        }

        public static void FixMaterialNameUnique(glTF GLTF)
        {
            // NOTE: Windows FileSystem は大文字小文字の違いは同名ファイルとして扱ってしまうため, IgnoreCase で評価する.
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var materialIdx = 0; materialIdx < GLTF.materials.Count; ++materialIdx)
            {
                var material = GLTF.materials[materialIdx];

                if (string.IsNullOrEmpty(material.name))
                {
                    material.name = $"material_{materialIdx}";
                }

                material.name = FixNameUnique(used, material.name);
            }
        }

        /// <summary>
        /// rename empty name to $"{index}"
        /// </summary>
        public static void FixNodeName(glTF GLTF)
        {
            for (var i = 0; i < GLTF.nodes.Count; ++i)
            {
                var node = GLTF.nodes[i];
                if (string.IsNullOrWhiteSpace(node.name))
                {
                    node.name = $"{i}";
                }
            }
        }

        public static void FixAnimationNameUnique(glTF GLTF)
        {
            // NOTE: Windows FileSystem は大文字小文字の違いは同名ファイルとして扱ってしまうため, IgnoreCase で評価する.
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < GLTF.animations.Count; ++i)
            {
                var animation = GLTF.animations[i];

                if (string.IsNullOrEmpty(animation.name))
                {
                    animation.name = $"animation_{i}";
                }

                animation.name = FixNameUnique(used, animation.name);
            }
        }

        public static void FixNodeNameUnique(glTF GLTF)
        {
            var m_uniqueNameSet = new HashSet<string>();
            int counter = 1;
            for (var i = 0; i < GLTF.nodes.Count; ++i)
            {
                RenameIfDupName(GLTF.nodes, i, m_uniqueNameSet, ref counter);
            }
        }

        static void RenameIfDupName(List<glTFNode> nodes, int index, HashSet<string> m_uniqueNameSet, ref int m_counter)
        {
            var t = nodes[index];

            if (!m_uniqueNameSet.Contains(t.name))
            {
                m_uniqueNameSet.Add(t.name);
                return;
            }

            var parent = nodes.FirstOrDefault(x => x.children != null && x.children.Contains(index));
            if (parent != null && (t.children == null || t.children.Length == 0))
            {
                /// AvatarBuilder:BuildHumanAvatar で同名の Transform があるとエラーになる。
                /// 
                /// AvatarBuilder 'GLTF': Ambiguous Transform '32/root/torso_1/torso_2/torso_3/torso_4/torso_5/torso_6/torso_7/neck_1/neck_2/head/ENDSITE' and '32/root/torso_1/torso_2/torso_3/torso_4/torso_5/torso_6/torso_7/l_shoulder/l_up_arm/l_low_arm/l_hand/ENDSITE' found in hierarchy for human bone 'Head'. Transform name mapped to a human bone must be unique.
                /// UnityEngine.AvatarBuilder:BuildHumanAvatar (UnityEngine.GameObject,UnityEngine.HumanDescription)
                /// UniHumanoid.AvatarDescription:CreateAvatar (UnityEngine.Transform) 
                /// 
                /// 主に BVH の EndSite 由来の GameObject 名が重複することへの対策
                ///  ex: parent-ENDSITE
                var newName = $"{parent.name}-{t.name}";
                if (!m_uniqueNameSet.Contains(newName))
                {
                    UniGLTFLogger.Warning($"force rename !!: {t.name} => {newName}");
                    t.name = newName;
                    m_uniqueNameSet.Add(newName);
                    return;
                }
            }

            // 連番
            for (int i = 0; i < 100; ++i)
            {
                // ex: name.1
                var newName = $"{t.name}{m_counter++}";
                if (!m_uniqueNameSet.Contains(newName))
                {
                    UniGLTFLogger.Warning($"force rename: {t.name} => {newName}");
                    t.name = newName;
                    m_uniqueNameSet.Add(newName);
                    return;
                }
            }

            throw new NotImplementedException();
        }
    }
}