using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UniGLTF.Utils;
using UnityEngine;


/// <summary>
/// * 胴体: hips, spine, chest, upperChest, neck, head => 6
/// * 腕: shoulder, upperArm, lowerArm, hand => 4 x 2
/// * 脚: upperLeg, lowerLeg, foot, toes => 4 x 2
///     = 22
/// * 指: proximal, inetermediate, distal => 3 x 5 x 2 = 30
/// </summary>
namespace VrmLib
{
    public struct HumanoidHead
    {
        // required
        public Node Head;
        public Node Jaw;
        public Node LeftEye;
        public Node RightEye;

        public bool HasRequiredBones => Head != null;
    };

    public struct HumanoidArm
    {
        public Node Shoulder;
        // required
        public Node Upper;
        // required
        public Node Lower;
        // required
        public Node Hand;

        public bool HasRequiredBones => Upper != null && Lower != null && Hand != null;

        public Vector3 Direction
        {
            get
            {
                if (Shoulder != null)
                {
                    return Vector3.Normalize(Hand.Translation - Shoulder.Translation);
                }
                else
                {
                    return Vector3.Normalize(Hand.Translation - Upper.Translation);
                }
            }
        }

        public void DirectTo(Vector3 dir)
        {
            if (Shoulder != null)
            {
                Shoulder.RotateFromTo(Upper.Translation - Shoulder.Translation, dir);
            }
            Upper.RotateFromTo(Lower.Translation - Upper.Translation, dir);
            Lower.RotateFromTo(Hand.Translation - Lower.Translation, dir);
        }
    };

    public struct HumanoidLeg
    {
        public Node Upper;
        public Node Lower;
        public Node Foot;
        public Node Toe;
    };

    struct HumanoidFinger
    {
        public Node Proximal;
        public Node Intermediate;
        public Node Distal;
    }

    struct HumanoidThumb
    {
        public Node Metacarpal;
        public Node Proximal;
        public Node Distal;
    }


    struct HumanoidFingers
    {
        public HumanoidThumb Thumb;
        public HumanoidFinger Index;
        public HumanoidFinger Middle;
        public HumanoidFinger Ring;
        public HumanoidFinger Little;
    };

    /// <summary>
    /// ヒューマノイドの姿勢を制御する
    /// </summary>
    public class Humanoid : IDictionary<HumanoidBones, Node>
    {
        public Node Hips;
        public Node Spine;
        public Node Chest;
        public Node UpperChest;
        public Node Neck;

        HumanoidHead Head;

        HumanoidArm LeftArm;
        HumanoidFingers LeftFingers;

        HumanoidArm RightArm;
        HumanoidFingers RightFingers;

        HumanoidLeg LeftLeg;
        HumanoidLeg RightLeg;

        public bool HasRequiredBones
        {
            get
            {
                if (Hips == null) return false;
                if (Spine == null) return false;
                if (!Head.HasRequiredBones) return false;
                if (!LeftArm.HasRequiredBones) return false;
                if (!RightArm.HasRequiredBones) return false;

                // TODO
                return true;
            }
        }

        ICollection<HumanoidBones> IDictionary<HumanoidBones, Node>.Keys => throw new System.NotImplementedException();

        ICollection<Node> IDictionary<HumanoidBones, Node>.Values => throw new System.NotImplementedException();

        int ICollection<KeyValuePair<HumanoidBones, Node>>.Count => this.Select(_ => _).Count();

        bool ICollection<KeyValuePair<HumanoidBones, Node>>.IsReadOnly => throw new System.NotImplementedException();

        Node IDictionary<HumanoidBones, Node>.this[HumanoidBones key] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public Humanoid()
        {
        }

        public Humanoid(Node root)
        {
            Assign(root);
        }

        public void Assign(Node root)
        {
            foreach (var node in root.Traverse())
            {
                if (node.HumanoidBone.HasValue)
                {
                    Add(node.HumanoidBone.Value, node);
                }
            }
        }

        void CopyTraverse(Node src, Node dst)
        {
            dst.HumanoidBone = src.HumanoidBone;
            dst.LocalScaling = src.LocalScaling;
            dst.LocalRotation = src.LocalRotation;
            dst.LocalTranslation = src.LocalTranslation;
            foreach (var child in src.Children)
            {
                var dstChild = new Node(child.Name /*+ ".copy"*/);
                dst.Add(dstChild);
                CopyTraverse(child, dstChild);
            }
        }

        public Humanoid CopyNodes()
        {
            // ヒエラルキーのコピーを作成する
            var hips = new Node(Hips.Name /*+ ".copy"*/);
            CopyTraverse(Hips, hips);

            var humanoid = new Humanoid(hips);
            return humanoid;
        }

        // Y軸180度回転
        public (bool, string) Y180()
        {
            var sb = new System.Text.StringBuilder();
            Hips.LocalRotation = Quaternion.Euler(0, MathFWrap.PI, 0);
            Hips.CalcWorldMatrix();
            return (true, sb.ToString());
        }

        /// <summary>
        /// 上半身のTPose
        /// </summary>
        public (bool, string) MakeTPose()
        {
            var sb = new System.Text.StringBuilder();
            bool modified = false;

            // hipsのforward を -Z に向ける
            // hipsのforward は (left.leg - right.leg) cross (0, 1, 0)
            var left = Vector3.Normalize(LeftLeg.Upper.Translation - RightLeg.Upper.Translation);
            var forward = Vector3.Cross(left, Vector3.up);
            if (Vector3.Dot(forward, -Vector3.forward) < 1.0f - 0.1f)
            {
                Hips.RotateFromTo(forward, -Vector3.forward);
                modified = true;
            }

            if (Vector3.Dot(LeftArm.Direction, -Vector3.right) < 1.0f - 0.1f)
            {
                LeftArm.DirectTo(-Vector3.right);
                sb.Append("(fix left arm)");
                modified = true;
            }
            if (Vector3.Dot(RightArm.Direction, Vector3.right) < 1.0f - 0.1f)
            {
                RightArm.DirectTo(Vector3.right);
                sb.Append("(fix right arm)");
                modified = true;
            }

            Hips.CalcWorldMatrix();
            return (modified, sb.ToString());
        }

        public void RetargetTo(Humanoid srcTPose, Humanoid dst)
        {
            foreach (var kv in this)
            {
                var tposeNode = srcTPose[kv.Key];
                if (dst.TryGetValue(kv.Key, out Node node))
                {
                    // var t = tposeNode.LocalRotation;
                    // var t = Quaternion.identity;
                    // node.LocalRotationWithoutUpdate = Quaternion.Inverse(t) * kv.Value.LocalRotation;
                    // node.LocalRotationWithoutUpdate = kv.Value.LocalRotation * Quaternion.Inverse(t);

                    var t = tposeNode.Rotation;
                    // node.Rotation = Quaternion.Inverse(t) * kv.Value.Rotation;
                    node.Rotation = kv.Value.Rotation * Quaternion.Inverse(t);
                    // node.LocalRotationWithoutUpdate = kv.Value.LocalRotation * Quaternion.Inverse(t);
                }
                else
                {
                    Console.WriteLine($"{kv.Key} not found");
                }
            }
            dst.Hips.CalcWorldMatrix();
        }

        #region interface
        public void Add(KeyValuePair<HumanoidBones, Node> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(HumanoidBones key, Node node)
        {
            if (key == HumanoidBones.unknown)
            {
                throw new ArgumentException();
            }

            node.HumanoidBone = key;

            switch (node.HumanoidBone.Value)
            {
                case HumanoidBones.hips: Hips = node; break;
                case HumanoidBones.spine: Spine = node; break;
                case HumanoidBones.chest: Chest = node; break;
                case HumanoidBones.upperChest: UpperChest = node; break;
                case HumanoidBones.neck: Neck = node; break;
                case HumanoidBones.head: Head.Head = node; break;
                case HumanoidBones.jaw: Head.Jaw = node; break;
                case HumanoidBones.leftEye: Head.LeftEye = node; break;
                case HumanoidBones.rightEye: Head.RightEye = node; break;

                case HumanoidBones.leftShoulder: LeftArm.Shoulder = node; break;
                case HumanoidBones.leftUpperArm: LeftArm.Upper = node; break;
                case HumanoidBones.leftLowerArm: LeftArm.Lower = node; break;
                case HumanoidBones.leftHand: LeftArm.Hand = node; break;

                case HumanoidBones.rightShoulder: RightArm.Shoulder = node; break;
                case HumanoidBones.rightUpperArm: RightArm.Upper = node; break;
                case HumanoidBones.rightLowerArm: RightArm.Lower = node; break;
                case HumanoidBones.rightHand: RightArm.Hand = node; break;

                case HumanoidBones.leftUpperLeg: LeftLeg.Upper = node; break;
                case HumanoidBones.leftLowerLeg: LeftLeg.Lower = node; break;
                case HumanoidBones.leftFoot: LeftLeg.Foot = node; break;
                case HumanoidBones.leftToes: LeftLeg.Toe = node; break;

                case HumanoidBones.rightUpperLeg: RightLeg.Upper = node; break;
                case HumanoidBones.rightLowerLeg: RightLeg.Lower = node; break;
                case HumanoidBones.rightFoot: RightLeg.Foot = node; break;
                case HumanoidBones.rightToes: RightLeg.Toe = node; break;

                case HumanoidBones.leftThumbMetacarpal: LeftFingers.Thumb.Metacarpal = node; break;
                case HumanoidBones.leftThumbProximal: LeftFingers.Thumb.Proximal = node; break;
                case HumanoidBones.leftThumbDistal: LeftFingers.Thumb.Distal = node; break;
                case HumanoidBones.leftIndexProximal: LeftFingers.Index.Proximal = node; break;
                case HumanoidBones.leftIndexIntermediate: LeftFingers.Index.Intermediate = node; break;
                case HumanoidBones.leftIndexDistal: LeftFingers.Index.Distal = node; break;
                case HumanoidBones.leftMiddleProximal: LeftFingers.Middle.Proximal = node; break;
                case HumanoidBones.leftMiddleIntermediate: LeftFingers.Middle.Intermediate = node; break;
                case HumanoidBones.leftMiddleDistal: LeftFingers.Middle.Distal = node; break;
                case HumanoidBones.leftRingProximal: LeftFingers.Ring.Proximal = node; break;
                case HumanoidBones.leftRingIntermediate: LeftFingers.Ring.Intermediate = node; break;
                case HumanoidBones.leftRingDistal: LeftFingers.Ring.Distal = node; break;
                case HumanoidBones.leftLittleProximal: LeftFingers.Little.Proximal = node; break;
                case HumanoidBones.leftLittleIntermediate: LeftFingers.Little.Intermediate = node; break;
                case HumanoidBones.leftLittleDistal: LeftFingers.Little.Distal = node; break;

                case HumanoidBones.rightThumbMetacarpal: RightFingers.Thumb.Metacarpal = node; break;
                case HumanoidBones.rightThumbProximal: RightFingers.Thumb.Proximal = node; break;
                case HumanoidBones.rightThumbDistal: RightFingers.Thumb.Distal = node; break;
                case HumanoidBones.rightIndexProximal: RightFingers.Index.Proximal = node; break;
                case HumanoidBones.rightIndexIntermediate: RightFingers.Index.Intermediate = node; break;
                case HumanoidBones.rightIndexDistal: RightFingers.Index.Distal = node; break;
                case HumanoidBones.rightMiddleProximal: RightFingers.Middle.Proximal = node; break;
                case HumanoidBones.rightMiddleIntermediate: RightFingers.Middle.Intermediate = node; break;
                case HumanoidBones.rightMiddleDistal: RightFingers.Middle.Distal = node; break;
                case HumanoidBones.rightRingProximal: RightFingers.Ring.Proximal = node; break;
                case HumanoidBones.rightRingIntermediate: RightFingers.Ring.Intermediate = node; break;
                case HumanoidBones.rightRingDistal: RightFingers.Ring.Distal = node; break;
                case HumanoidBones.rightLittleProximal: RightFingers.Little.Proximal = node; break;
                case HumanoidBones.rightLittleIntermediate: RightFingers.Little.Intermediate = node; break;
                case HumanoidBones.rightLittleDistal: RightFingers.Little.Distal = node; break;

                default: throw new NotImplementedException();
            }
        }

        public bool ContainsKey(HumanoidBones key)
        {
            return TryGetValue(key, out Node _);
        }

        public bool Remove(HumanoidBones key)
        {
            if (!ContainsKey(key))
            {
                return false;
            }
            Add(key, null);
            return true;
        }

        public Node this[HumanoidBones key]
        {
            get
            {
                if (TryGetValue(key, out Node node))
                {
                    return node;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        public Node this[Node node]
        {
            get
            {
                if (node.HumanoidBone.HasValue && node.HumanoidBone.Value != HumanoidBones.unknown)
                {
                    return this[node.HumanoidBone.Value];
                }
                else
                {
                    // とりあえず
                    return Hips.Traverse().First(x => x.Name == node.Name);
                }
            }
        }

        public bool TryGetValue(HumanoidBones key, out Node value)
        {
            switch (key)
            {
                case HumanoidBones.hips: value = Hips; return true;
                case HumanoidBones.spine: value = Spine; return true;
                case HumanoidBones.chest: value = Chest; return true;
                case HumanoidBones.upperChest: value = UpperChest; return true;
                case HumanoidBones.neck: value = Neck; return true;
                case HumanoidBones.head: value = Head.Head; return true;
                case HumanoidBones.jaw: value = Head.Jaw; return true;
                case HumanoidBones.leftEye: value = Head.LeftEye; return true;
                case HumanoidBones.rightEye: value = Head.RightEye; return true;

                case HumanoidBones.leftShoulder: value = LeftArm.Shoulder; return true;
                case HumanoidBones.leftUpperArm: value = LeftArm.Upper; return true;
                case HumanoidBones.leftLowerArm: value = LeftArm.Lower; return true;
                case HumanoidBones.leftHand: value = LeftArm.Hand; return true;

                case HumanoidBones.rightShoulder: value = RightArm.Shoulder; return true;
                case HumanoidBones.rightUpperArm: value = RightArm.Upper; return true;
                case HumanoidBones.rightLowerArm: value = RightArm.Lower; return true;
                case HumanoidBones.rightHand: value = RightArm.Hand; return true;

                case HumanoidBones.leftUpperLeg: value = LeftLeg.Upper; return true;
                case HumanoidBones.leftLowerLeg: value = LeftLeg.Lower; return true;
                case HumanoidBones.leftFoot: value = LeftLeg.Foot; return true;
                case HumanoidBones.leftToes: value = LeftLeg.Toe; return true;

                case HumanoidBones.rightUpperLeg: value = RightLeg.Upper; return true;
                case HumanoidBones.rightLowerLeg: value = RightLeg.Lower; return true;
                case HumanoidBones.rightFoot: value = RightLeg.Foot; return true;
                case HumanoidBones.rightToes: value = RightLeg.Toe; return true;

                case HumanoidBones.leftThumbMetacarpal: value = LeftFingers.Thumb.Metacarpal; return true;
                case HumanoidBones.leftThumbProximal: value = LeftFingers.Thumb.Proximal; return true;
                case HumanoidBones.leftThumbDistal: value = LeftFingers.Thumb.Distal; return true;
                case HumanoidBones.leftIndexProximal: value = LeftFingers.Index.Proximal; return true;
                case HumanoidBones.leftIndexIntermediate: value = LeftFingers.Index.Intermediate; return true;
                case HumanoidBones.leftIndexDistal: value = LeftFingers.Index.Distal; return true;
                case HumanoidBones.leftMiddleProximal: value = LeftFingers.Middle.Proximal; return true;
                case HumanoidBones.leftMiddleIntermediate: value = LeftFingers.Middle.Intermediate; return true;
                case HumanoidBones.leftMiddleDistal: value = LeftFingers.Middle.Distal; return true;
                case HumanoidBones.leftRingProximal: value = LeftFingers.Ring.Proximal; return true;
                case HumanoidBones.leftRingIntermediate: value = LeftFingers.Ring.Intermediate; return true;
                case HumanoidBones.leftRingDistal: value = LeftFingers.Ring.Distal; return true;
                case HumanoidBones.leftLittleProximal: value = LeftFingers.Little.Proximal; return true;
                case HumanoidBones.leftLittleIntermediate: value = LeftFingers.Little.Intermediate; return true;
                case HumanoidBones.leftLittleDistal: value = LeftFingers.Little.Distal; return true;

                case HumanoidBones.rightThumbProximal: value = LeftFingers.Thumb.Proximal; return true;
                case HumanoidBones.rightThumbMetacarpal: value = LeftFingers.Thumb.Metacarpal; return true;
                case HumanoidBones.rightThumbDistal: value = LeftFingers.Thumb.Distal; return true;
                case HumanoidBones.rightIndexProximal: value = LeftFingers.Index.Proximal; return true;
                case HumanoidBones.rightIndexIntermediate: value = LeftFingers.Index.Intermediate; return true;
                case HumanoidBones.rightIndexDistal: value = LeftFingers.Index.Distal; return true;
                case HumanoidBones.rightMiddleProximal: value = LeftFingers.Middle.Proximal; return true;
                case HumanoidBones.rightMiddleIntermediate: value = LeftFingers.Middle.Intermediate; return true;
                case HumanoidBones.rightMiddleDistal: value = LeftFingers.Middle.Distal; return true;
                case HumanoidBones.rightRingProximal: value = LeftFingers.Ring.Proximal; return true;
                case HumanoidBones.rightRingIntermediate: value = LeftFingers.Ring.Intermediate; return true;
                case HumanoidBones.rightRingDistal: value = LeftFingers.Ring.Distal; return true;
                case HumanoidBones.rightLittleProximal: value = LeftFingers.Little.Proximal; return true;
                case HumanoidBones.rightLittleIntermediate: value = LeftFingers.Little.Intermediate; return true;
                case HumanoidBones.rightLittleDistal: value = LeftFingers.Little.Distal; return true;
            }

            value = null;
            return false;
        }

        public void Clear()
        {
            foreach (HumanoidBones key in CachedEnum.GetValues<HumanoidBones>())
            {
                Add(key, null);
            }
        }

        public IEnumerator<KeyValuePair<HumanoidBones, Node>> GetEnumerator()
        {
            if (Hips != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.hips, Hips);
            if (Spine != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.spine, Spine);
            if (Chest != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.chest, Chest);
            if (UpperChest != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.upperChest, UpperChest);
            if (Neck != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.neck, Neck);
            if (Head.Head != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.head, Head.Head);

            if (LeftArm.Shoulder != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.leftShoulder, LeftArm.Shoulder);
            if (LeftArm.Upper != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.leftUpperArm, LeftArm.Upper);
            if (LeftArm.Lower != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.leftLowerArm, LeftArm.Lower);
            if (LeftArm.Hand != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.leftHand, LeftArm.Hand);

            if (RightArm.Shoulder != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.rightShoulder, RightArm.Shoulder);
            if (RightArm.Upper != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.rightUpperArm, RightArm.Upper);
            if (RightArm.Lower != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.rightLowerArm, RightArm.Lower);
            if (RightArm.Hand != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.rightHand, RightArm.Hand);

            if (LeftLeg.Upper != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.leftUpperLeg, LeftLeg.Upper);
            if (LeftLeg.Lower != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.leftLowerLeg, LeftLeg.Lower);
            if (LeftLeg.Foot != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.leftFoot, LeftLeg.Foot);

            if (RightLeg.Upper != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.rightUpperLeg, RightLeg.Upper);
            if (RightLeg.Lower != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.rightLowerLeg, RightLeg.Lower);
            if (RightLeg.Foot != null) yield return new KeyValuePair<HumanoidBones, Node>(HumanoidBones.rightFoot, RightLeg.Foot);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region NotImplement
        bool ICollection<KeyValuePair<HumanoidBones, Node>>.Contains(KeyValuePair<HumanoidBones, Node> item)
        {
            throw new System.NotImplementedException();
        }

        void ICollection<KeyValuePair<HumanoidBones, Node>>.CopyTo(KeyValuePair<HumanoidBones, Node>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        bool ICollection<KeyValuePair<HumanoidBones, Node>>.Remove(KeyValuePair<HumanoidBones, Node> item)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #endregion
    }
}
