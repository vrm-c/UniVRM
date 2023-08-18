using System;
using System.Collections.Generic;
using UniHumanoid;
using UnityEngine;

namespace UniVRM10
{
    public class Vrm10AnimationInstance : MonoBehaviour, IVrm10Animation
    {
        public SkinnedMeshRenderer BoxMan;
        public void ShowBoxMan(bool enable)
        {
            BoxMan.enabled = enable;
        }

        public void Dispose()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }
            else
            {
                DestroyImmediate(this);
            }
        }

        public (INormalizedPoseProvider, ITPoseProvider) ControlRig { get; set; }

        readonly Dictionary<ExpressionKey, Func<float>> _ExpressionGetterMap = new();
        public IReadOnlyDictionary<ExpressionKey, Func<float>> ExpressionMap => _ExpressionGetterMap;

        readonly Dictionary<ExpressionKey, Action<float>> _ExpressionSetterMap = new();
        public IReadOnlyDictionary<ExpressionKey, Action<float>> ExpressionSetterMap => _ExpressionSetterMap;

        public LookAtInput? LookAt { get; set; }

        void InitializeExpression(ExpressionKey key, Func<float> getter, Action<float> setter)
        {
            _ExpressionGetterMap.Add(key, getter);
            _ExpressionSetterMap.Add(key, setter);
        }

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
                    case ExpressionPreset.happy: InitializeExpression(key, () => preset_happy, (value) => preset_happy = value); break;
                    case ExpressionPreset.angry: InitializeExpression(key, () => preset_angry, (value) => preset_angry = value); break;
                    case ExpressionPreset.sad: InitializeExpression(key, () => preset_sad, (value) => preset_sad = value); break;
                    case ExpressionPreset.relaxed: InitializeExpression(key, () => preset_relaxed, (value) => preset_relaxed = value); break;
                    case ExpressionPreset.surprised: InitializeExpression(key, () => preset_surprised, (value) => preset_surprised = value); break;
                    case ExpressionPreset.aa: InitializeExpression(key, () => preset_aa, (value) => preset_aa = value); break;
                    case ExpressionPreset.ih: InitializeExpression(key, () => preset_ih, (value) => preset_ih = value); break;
                    case ExpressionPreset.ou: InitializeExpression(key, () => preset_ou, (value) => preset_ou = value); break;
                    case ExpressionPreset.ee: InitializeExpression(key, () => preset_ee, (value) => preset_ee = value); break;
                    case ExpressionPreset.oh: InitializeExpression(key, () => preset_oh, (value) => preset_oh = value); break;
                    case ExpressionPreset.blink: InitializeExpression(key, () => preset_blink, (value) => preset_blink = value); break;
                    case ExpressionPreset.blinkLeft: InitializeExpression(key, () => preset_blinkleft, (value) => preset_blinkleft = value); break;
                    case ExpressionPreset.blinkRight: InitializeExpression(key, () => preset_blinkright, (value) => preset_blinkright = value); break;
                    // case ExpressionPreset.lookUp: _ExpressionMap.Add(key, () => preset_lookUp); break;
                    // case ExpressionPreset.lookDown: _ExpressionMap.Add(key, () => preset_lookDown); break;
                    // case ExpressionPreset.lookLeft: _ExpressionMap.Add(key, () => preset_lookLeft); break;
                    // case ExpressionPreset.lookRight: _ExpressionMap.Add(key, () => preset_lookRight); break;
                    case ExpressionPreset.neutral: InitializeExpression(key, () => preset_neutral, (value) => preset_neutral = value); break;
                    case ExpressionPreset.custom:
                        {
                            switch (customIndex++)
                            {
                                case 00: InitializeExpression(key, () => custom_00, (value) => custom_00 = value); break;
                                case 01: InitializeExpression(key, () => custom_01, (value) => custom_01 = value); break;
                                case 02: InitializeExpression(key, () => custom_02, (value) => custom_02 = value); break;
                                case 03: InitializeExpression(key, () => custom_03, (value) => custom_03 = value); break;
                                case 04: InitializeExpression(key, () => custom_04, (value) => custom_04 = value); break;
                                case 05: InitializeExpression(key, () => custom_05, (value) => custom_05 = value); break;
                                case 06: InitializeExpression(key, () => custom_06, (value) => custom_06 = value); break;
                                case 07: InitializeExpression(key, () => custom_07, (value) => custom_07 = value); break;
                                case 08: InitializeExpression(key, () => custom_08, (value) => custom_08 = value); break;
                                case 09: InitializeExpression(key, () => custom_09, (value) => custom_09 = value); break;

                                case 10: InitializeExpression(key, () => custom_10, (value) => custom_10 = value); break;
                                case 11: InitializeExpression(key, () => custom_11, (value) => custom_11 = value); break;
                                case 12: InitializeExpression(key, () => custom_12, (value) => custom_12 = value); break;
                                case 13: InitializeExpression(key, () => custom_13, (value) => custom_13 = value); break;
                                case 14: InitializeExpression(key, () => custom_14, (value) => custom_14 = value); break;
                                case 15: InitializeExpression(key, () => custom_15, (value) => custom_15 = value); break;
                                case 16: InitializeExpression(key, () => custom_16, (value) => custom_16 = value); break;
                                case 17: InitializeExpression(key, () => custom_17, (value) => custom_17 = value); break;
                                case 18: InitializeExpression(key, () => custom_18, (value) => custom_18 = value); break;
                                case 19: InitializeExpression(key, () => custom_19, (value) => custom_19 = value); break;

                                case 20: InitializeExpression(key, () => custom_20, (value) => custom_20 = value); break;
                                case 21: InitializeExpression(key, () => custom_21, (value) => custom_21 = value); break;
                                case 22: InitializeExpression(key, () => custom_22, (value) => custom_22 = value); break;
                                case 23: InitializeExpression(key, () => custom_23, (value) => custom_23 = value); break;
                                case 24: InitializeExpression(key, () => custom_24, (value) => custom_24 = value); break;
                                case 25: InitializeExpression(key, () => custom_25, (value) => custom_25 = value); break;
                                case 26: InitializeExpression(key, () => custom_26, (value) => custom_26 = value); break;
                                case 27: InitializeExpression(key, () => custom_27, (value) => custom_27 = value); break;
                                case 28: InitializeExpression(key, () => custom_28, (value) => custom_28 = value); break;
                                case 29: InitializeExpression(key, () => custom_29, (value) => custom_29 = value); break;

                                case 30: InitializeExpression(key, () => custom_30, (value) => custom_30 = value); break;
                                case 31: InitializeExpression(key, () => custom_31, (value) => custom_31 = value); break;
                                case 32: InitializeExpression(key, () => custom_32, (value) => custom_32 = value); break;
                                case 33: InitializeExpression(key, () => custom_33, (value) => custom_33 = value); break;
                                case 34: InitializeExpression(key, () => custom_34, (value) => custom_34 = value); break;
                                case 35: InitializeExpression(key, () => custom_35, (value) => custom_35 = value); break;
                                case 36: InitializeExpression(key, () => custom_36, (value) => custom_36 = value); break;
                                case 37: InitializeExpression(key, () => custom_37, (value) => custom_37 = value); break;
                                case 38: InitializeExpression(key, () => custom_38, (value) => custom_38 = value); break;
                                case 39: InitializeExpression(key, () => custom_39, (value) => custom_39 = value); break;

                                case 40: InitializeExpression(key, () => custom_40, (value) => custom_40 = value); break;
                                case 41: InitializeExpression(key, () => custom_41, (value) => custom_41 = value); break;
                                case 42: InitializeExpression(key, () => custom_42, (value) => custom_42 = value); break;
                                case 43: InitializeExpression(key, () => custom_43, (value) => custom_43 = value); break;
                                case 44: InitializeExpression(key, () => custom_44, (value) => custom_44 = value); break;
                                case 45: InitializeExpression(key, () => custom_45, (value) => custom_45 = value); break;
                                case 46: InitializeExpression(key, () => custom_46, (value) => custom_46 = value); break;
                                case 47: InitializeExpression(key, () => custom_47, (value) => custom_47 = value); break;
                                case 48: InitializeExpression(key, () => custom_48, (value) => custom_48 = value); break;
                                case 49: InitializeExpression(key, () => custom_49, (value) => custom_49 = value); break;

                                case 50: InitializeExpression(key, () => custom_50, (value) => custom_50 = value); break;
                                case 51: InitializeExpression(key, () => custom_51, (value) => custom_51 = value); break;
                                case 52: InitializeExpression(key, () => custom_52, (value) => custom_52 = value); break;
                                case 53: InitializeExpression(key, () => custom_53, (value) => custom_53 = value); break;
                                case 54: InitializeExpression(key, () => custom_54, (value) => custom_54 = value); break;
                                case 55: InitializeExpression(key, () => custom_55, (value) => custom_55 = value); break;
                                case 56: InitializeExpression(key, () => custom_56, (value) => custom_56 = value); break;
                                case 57: InitializeExpression(key, () => custom_57, (value) => custom_57 = value); break;
                                case 58: InitializeExpression(key, () => custom_58, (value) => custom_58 = value); break;
                                case 59: InitializeExpression(key, () => custom_59, (value) => custom_59 = value); break;

                                case 60: InitializeExpression(key, () => custom_60, (value) => custom_60 = value); break;
                                case 61: InitializeExpression(key, () => custom_61, (value) => custom_61 = value); break;
                                case 62: InitializeExpression(key, () => custom_62, (value) => custom_62 = value); break;
                                case 63: InitializeExpression(key, () => custom_63, (value) => custom_63 = value); break;
                                case 64: InitializeExpression(key, () => custom_64, (value) => custom_64 = value); break;
                                case 65: InitializeExpression(key, () => custom_65, (value) => custom_65 = value); break;
                                case 66: InitializeExpression(key, () => custom_66, (value) => custom_66 = value); break;
                                case 67: InitializeExpression(key, () => custom_67, (value) => custom_67 = value); break;
                                case 68: InitializeExpression(key, () => custom_68, (value) => custom_68 = value); break;
                                case 69: InitializeExpression(key, () => custom_69, (value) => custom_69 = value); break;

                                case 70: InitializeExpression(key, () => custom_70, (value) => custom_70 = value); break;
                                case 71: InitializeExpression(key, () => custom_71, (value) => custom_71 = value); break;
                                case 72: InitializeExpression(key, () => custom_72, (value) => custom_72 = value); break;
                                case 73: InitializeExpression(key, () => custom_73, (value) => custom_73 = value); break;
                                case 74: InitializeExpression(key, () => custom_74, (value) => custom_74 = value); break;
                                case 75: InitializeExpression(key, () => custom_75, (value) => custom_75 = value); break;
                                case 76: InitializeExpression(key, () => custom_76, (value) => custom_76 = value); break;
                                case 77: InitializeExpression(key, () => custom_77, (value) => custom_77 = value); break;
                                case 78: InitializeExpression(key, () => custom_78, (value) => custom_78 = value); break;
                                case 79: InitializeExpression(key, () => custom_79, (value) => custom_79 = value); break;

                                case 80: InitializeExpression(key, () => custom_80, (value) => custom_80 = value); break;
                                case 81: InitializeExpression(key, () => custom_81, (value) => custom_81 = value); break;
                                case 82: InitializeExpression(key, () => custom_82, (value) => custom_82 = value); break;
                                case 83: InitializeExpression(key, () => custom_83, (value) => custom_83 = value); break;
                                case 84: InitializeExpression(key, () => custom_84, (value) => custom_84 = value); break;
                                case 85: InitializeExpression(key, () => custom_85, (value) => custom_85 = value); break;
                                case 86: InitializeExpression(key, () => custom_86, (value) => custom_86 = value); break;
                                case 87: InitializeExpression(key, () => custom_87, (value) => custom_87 = value); break;
                                case 88: InitializeExpression(key, () => custom_88, (value) => custom_88 = value); break;
                                case 89: InitializeExpression(key, () => custom_89, (value) => custom_89 = value); break;

                                case 90: InitializeExpression(key, () => custom_90, (value) => custom_90 = value); break;
                                case 91: InitializeExpression(key, () => custom_91, (value) => custom_91 = value); break;
                                case 92: InitializeExpression(key, () => custom_92, (value) => custom_92 = value); break;
                                case 93: InitializeExpression(key, () => custom_93, (value) => custom_93 = value); break;
                                case 94: InitializeExpression(key, () => custom_94, (value) => custom_94 = value); break;
                                case 95: InitializeExpression(key, () => custom_95, (value) => custom_95 = value); break;
                                case 96: InitializeExpression(key, () => custom_96, (value) => custom_96 = value); break;
                                case 97: InitializeExpression(key, () => custom_97, (value) => custom_97 = value); break;
                                case 98: InitializeExpression(key, () => custom_98, (value) => custom_98 = value); break;
                                case 99: InitializeExpression(key, () => custom_99, (value) => custom_99 = value); break;
                            }
                            break;
                        }
                }
            }
        }
    }
}
