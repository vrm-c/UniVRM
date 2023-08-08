using System;
using System.Collections.Generic;
using UniHumanoid;
using UnityEngine;

namespace UniVRM10
{
    public class VrmAnimationInstance : MonoBehaviour
    {
        public SkinnedMeshRenderer BoxMan;
        public (INormalizedPoseProvider, ITPoseProvider) ControlRig;
        public Dictionary<ExpressionKey, Func<float>> ExpressionMap = new();

        public float preset_happy;
        public float preset_angry;
        public float preset_sad;
        public float preset_relaxed;
        public float preset_surprised;
        public float preset_aa;
        public float preset_ih;
        public float preset_ou;
        public float preset_ee;
        public float preset_oh;
        public float preset_blink;
        public float preset_blinkleft;
        public float preset_blinkright;
        // public float preset_lookup;
        // public float preset_lookdown;
        // public float preset_lookleft;
        // public float preset_lookright;
        public float preset_neutral;

        public float custom_00;
        public float custom_01;
        public float custom_02;
        public float custom_03;
        public float custom_04;
        public float custom_05;
        public float custom_06;
        public float custom_07;
        public float custom_08;
        public float custom_09;

        public float custom_10;
        public float custom_11;
        public float custom_12;
        public float custom_13;
        public float custom_14;
        public float custom_15;
        public float custom_16;
        public float custom_17;
        public float custom_18;
        public float custom_19;

        public float custom_20;
        public float custom_21;
        public float custom_22;
        public float custom_23;
        public float custom_24;
        public float custom_25;
        public float custom_26;
        public float custom_27;
        public float custom_28;
        public float custom_29;

        public float custom_30;
        public float custom_31;
        public float custom_32;
        public float custom_33;
        public float custom_34;
        public float custom_35;
        public float custom_36;
        public float custom_37;
        public float custom_38;
        public float custom_39;

        public float custom_40;
        public float custom_41;
        public float custom_42;
        public float custom_43;
        public float custom_44;
        public float custom_45;
        public float custom_46;
        public float custom_47;
        public float custom_48;
        public float custom_49;

        public float custom_50;
        public float custom_51;
        public float custom_52;
        public float custom_53;
        public float custom_54;
        public float custom_55;
        public float custom_56;
        public float custom_57;
        public float custom_58;
        public float custom_59;

        public float custom_60;
        public float custom_61;
        public float custom_62;
        public float custom_63;
        public float custom_64;
        public float custom_65;
        public float custom_66;
        public float custom_67;
        public float custom_68;
        public float custom_69;

        public float custom_70;
        public float custom_71;
        public float custom_72;
        public float custom_73;
        public float custom_74;
        public float custom_75;
        public float custom_76;
        public float custom_77;
        public float custom_78;
        public float custom_79;

        public float custom_80;
        public float custom_81;
        public float custom_82;
        public float custom_83;
        public float custom_84;
        public float custom_85;
        public float custom_86;
        public float custom_87;
        public float custom_88;
        public float custom_89;

        public float custom_90;
        public float custom_91;
        public float custom_92;
        public float custom_93;
        public float custom_94;
        public float custom_95;
        public float custom_96;
        public float custom_97;
        public float custom_98;
        public float custom_99;

        public void Initialize(IEnumerable<ExpressionKey> keys)
        {
            var humanoid = gameObject.AddComponent<Humanoid>();
            if (humanoid.AssignBonesFromAnimator())
            {
                // require: transform is T-Pose 
                var provider = new InitRotationPoseProvider(transform, humanoid);
                ControlRig = (provider, provider);

                // create SkinnedMesh for bone visualize
                var animator = GetComponent<Animator>();
                BoxMan = SkeletonMeshUtility.CreateRenderer(animator);
                var material = new Material(Shader.Find("Standard"));
                BoxMan.sharedMaterial = material;
                var mesh = BoxMan.sharedMesh;
                mesh.name = "box-man";
            }

            int customIndex = 0;
            foreach (var key in keys)
            {
                switch (key.Preset)
                {
                    case ExpressionPreset.happy: ExpressionMap.Add(key, () => preset_happy); break;
                    case ExpressionPreset.angry: ExpressionMap.Add(key, () => preset_angry); break;
                    case ExpressionPreset.sad: ExpressionMap.Add(key, () => preset_sad); break;
                    case ExpressionPreset.relaxed: ExpressionMap.Add(key, () => preset_relaxed); break;
                    case ExpressionPreset.surprised: ExpressionMap.Add(key, () => preset_surprised); break;
                    case ExpressionPreset.aa: ExpressionMap.Add(key, () => preset_aa); break;
                    case ExpressionPreset.ih: ExpressionMap.Add(key, () => preset_ih); break;
                    case ExpressionPreset.ou: ExpressionMap.Add(key, () => preset_ou); break;
                    case ExpressionPreset.ee: ExpressionMap.Add(key, () => preset_ee); break;
                    case ExpressionPreset.oh: ExpressionMap.Add(key, () => preset_oh); break;
                    case ExpressionPreset.blink: ExpressionMap.Add(key, () => preset_blink); break;
                    case ExpressionPreset.blinkLeft: ExpressionMap.Add(key, () => preset_blinkleft); break;
                    case ExpressionPreset.blinkRight: ExpressionMap.Add(key, () => preset_blinkright); break;
                    // case ExpressionPreset.lookUp: ExpressionMap.Add(key, () => preset_lookUp); break;
                    // case ExpressionPreset.lookDown: ExpressionMap.Add(key, () => preset_lookDown); break;
                    // case ExpressionPreset.lookLeft: ExpressionMap.Add(key, () => preset_lookLeft); break;
                    // case ExpressionPreset.lookRight: ExpressionMap.Add(key, () => preset_lookRight); break;
                    case ExpressionPreset.neutral: ExpressionMap.Add(key, () => preset_neutral); break;
                    case ExpressionPreset.custom:
                        {
                            switch (customIndex++)
                            {
                                case 00: ExpressionMap.Add(key, () => custom_00); break;
                                case 01: ExpressionMap.Add(key, () => custom_01); break;
                                case 02: ExpressionMap.Add(key, () => custom_02); break;
                                case 03: ExpressionMap.Add(key, () => custom_03); break;
                                case 04: ExpressionMap.Add(key, () => custom_04); break;
                                case 05: ExpressionMap.Add(key, () => custom_05); break;
                                case 06: ExpressionMap.Add(key, () => custom_06); break;
                                case 07: ExpressionMap.Add(key, () => custom_07); break;
                                case 08: ExpressionMap.Add(key, () => custom_08); break;
                                case 09: ExpressionMap.Add(key, () => custom_09); break;

                                case 10: ExpressionMap.Add(key, () => custom_10); break;
                                case 11: ExpressionMap.Add(key, () => custom_11); break;
                                case 12: ExpressionMap.Add(key, () => custom_12); break;
                                case 13: ExpressionMap.Add(key, () => custom_13); break;
                                case 14: ExpressionMap.Add(key, () => custom_14); break;
                                case 15: ExpressionMap.Add(key, () => custom_15); break;
                                case 16: ExpressionMap.Add(key, () => custom_16); break;
                                case 17: ExpressionMap.Add(key, () => custom_17); break;
                                case 18: ExpressionMap.Add(key, () => custom_18); break;
                                case 19: ExpressionMap.Add(key, () => custom_19); break;

                                case 20: ExpressionMap.Add(key, () => custom_20); break;
                                case 21: ExpressionMap.Add(key, () => custom_21); break;
                                case 22: ExpressionMap.Add(key, () => custom_22); break;
                                case 23: ExpressionMap.Add(key, () => custom_23); break;
                                case 24: ExpressionMap.Add(key, () => custom_24); break;
                                case 25: ExpressionMap.Add(key, () => custom_25); break;
                                case 26: ExpressionMap.Add(key, () => custom_26); break;
                                case 27: ExpressionMap.Add(key, () => custom_27); break;
                                case 28: ExpressionMap.Add(key, () => custom_28); break;
                                case 29: ExpressionMap.Add(key, () => custom_29); break;

                                case 30: ExpressionMap.Add(key, () => custom_30); break;
                                case 31: ExpressionMap.Add(key, () => custom_31); break;
                                case 32: ExpressionMap.Add(key, () => custom_32); break;
                                case 33: ExpressionMap.Add(key, () => custom_33); break;
                                case 34: ExpressionMap.Add(key, () => custom_34); break;
                                case 35: ExpressionMap.Add(key, () => custom_35); break;
                                case 36: ExpressionMap.Add(key, () => custom_36); break;
                                case 37: ExpressionMap.Add(key, () => custom_37); break;
                                case 38: ExpressionMap.Add(key, () => custom_38); break;
                                case 39: ExpressionMap.Add(key, () => custom_39); break;

                                case 40: ExpressionMap.Add(key, () => custom_40); break;
                                case 41: ExpressionMap.Add(key, () => custom_41); break;
                                case 42: ExpressionMap.Add(key, () => custom_42); break;
                                case 43: ExpressionMap.Add(key, () => custom_43); break;
                                case 44: ExpressionMap.Add(key, () => custom_44); break;
                                case 45: ExpressionMap.Add(key, () => custom_45); break;
                                case 46: ExpressionMap.Add(key, () => custom_46); break;
                                case 47: ExpressionMap.Add(key, () => custom_47); break;
                                case 48: ExpressionMap.Add(key, () => custom_48); break;
                                case 49: ExpressionMap.Add(key, () => custom_49); break;

                                case 50: ExpressionMap.Add(key, () => custom_50); break;
                                case 51: ExpressionMap.Add(key, () => custom_51); break;
                                case 52: ExpressionMap.Add(key, () => custom_52); break;
                                case 53: ExpressionMap.Add(key, () => custom_53); break;
                                case 54: ExpressionMap.Add(key, () => custom_54); break;
                                case 55: ExpressionMap.Add(key, () => custom_55); break;
                                case 56: ExpressionMap.Add(key, () => custom_56); break;
                                case 57: ExpressionMap.Add(key, () => custom_57); break;
                                case 58: ExpressionMap.Add(key, () => custom_58); break;
                                case 59: ExpressionMap.Add(key, () => custom_59); break;

                                case 60: ExpressionMap.Add(key, () => custom_60); break;
                                case 61: ExpressionMap.Add(key, () => custom_61); break;
                                case 62: ExpressionMap.Add(key, () => custom_62); break;
                                case 63: ExpressionMap.Add(key, () => custom_63); break;
                                case 64: ExpressionMap.Add(key, () => custom_64); break;
                                case 65: ExpressionMap.Add(key, () => custom_65); break;
                                case 66: ExpressionMap.Add(key, () => custom_66); break;
                                case 67: ExpressionMap.Add(key, () => custom_67); break;
                                case 68: ExpressionMap.Add(key, () => custom_68); break;
                                case 69: ExpressionMap.Add(key, () => custom_69); break;

                                case 70: ExpressionMap.Add(key, () => custom_70); break;
                                case 71: ExpressionMap.Add(key, () => custom_71); break;
                                case 72: ExpressionMap.Add(key, () => custom_72); break;
                                case 73: ExpressionMap.Add(key, () => custom_73); break;
                                case 74: ExpressionMap.Add(key, () => custom_74); break;
                                case 75: ExpressionMap.Add(key, () => custom_75); break;
                                case 76: ExpressionMap.Add(key, () => custom_76); break;
                                case 77: ExpressionMap.Add(key, () => custom_77); break;
                                case 78: ExpressionMap.Add(key, () => custom_78); break;
                                case 79: ExpressionMap.Add(key, () => custom_79); break;

                                case 80: ExpressionMap.Add(key, () => custom_80); break;
                                case 81: ExpressionMap.Add(key, () => custom_81); break;
                                case 82: ExpressionMap.Add(key, () => custom_82); break;
                                case 83: ExpressionMap.Add(key, () => custom_83); break;
                                case 84: ExpressionMap.Add(key, () => custom_84); break;
                                case 85: ExpressionMap.Add(key, () => custom_85); break;
                                case 86: ExpressionMap.Add(key, () => custom_86); break;
                                case 87: ExpressionMap.Add(key, () => custom_87); break;
                                case 88: ExpressionMap.Add(key, () => custom_88); break;
                                case 89: ExpressionMap.Add(key, () => custom_89); break;

                                case 90: ExpressionMap.Add(key, () => custom_90); break;
                                case 91: ExpressionMap.Add(key, () => custom_91); break;
                                case 92: ExpressionMap.Add(key, () => custom_92); break;
                                case 93: ExpressionMap.Add(key, () => custom_93); break;
                                case 94: ExpressionMap.Add(key, () => custom_94); break;
                                case 95: ExpressionMap.Add(key, () => custom_95); break;
                                case 96: ExpressionMap.Add(key, () => custom_96); break;
                                case 97: ExpressionMap.Add(key, () => custom_97); break;
                                case 98: ExpressionMap.Add(key, () => custom_98); break;
                                case 99: ExpressionMap.Add(key, () => custom_99); break;
                            }
                            break;
                        }
                }
            }
        }
    }
}
