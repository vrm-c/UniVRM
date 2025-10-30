using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public sealed class VRM10ObjectExpression
    {
        #region Preset
        [SerializeField, Header("Emotion")]
        public VRM10Expression Happy;

        [SerializeField]
        public VRM10Expression Angry;

        [SerializeField]
        public VRM10Expression Sad;

        [SerializeField]
        public VRM10Expression Relaxed;

        [SerializeField]
        public VRM10Expression Surprised;

        [SerializeField, Header("LipSync")]
        public VRM10Expression Aa;

        [SerializeField]
        public VRM10Expression Ih;

        [SerializeField]
        public VRM10Expression Ou;

        [SerializeField]
        public VRM10Expression Ee;

        [SerializeField]
        public VRM10Expression Oh;

        [SerializeField, Header("Blink")]
        public VRM10Expression Blink;

        [SerializeField]
        public VRM10Expression BlinkLeft;

        [SerializeField]
        public VRM10Expression BlinkRight;

        [SerializeField, Header("LookAt")]
        public VRM10Expression LookUp;

        [SerializeField]
        public VRM10Expression LookDown;

        [SerializeField]
        public VRM10Expression LookLeft;

        [SerializeField]
        public VRM10Expression LookRight;

        [SerializeField, Header("Other")]
        public VRM10Expression Neutral;
        #endregion

        [SerializeField]
        public List<VRM10Expression> CustomClips = new List<VRM10Expression>();

        public IEnumerable<(ExpressionPreset Preset, VRM10Expression Clip)> Clips
        {
            get
            {
                if (Happy != null) yield return (ExpressionPreset.happy, Happy);
                if (Angry != null) yield return (ExpressionPreset.angry, Angry);
                if (Sad != null) yield return (ExpressionPreset.sad, Sad);
                if (Relaxed != null) yield return (ExpressionPreset.relaxed, Relaxed);
                if (Surprised != null) yield return (ExpressionPreset.surprised, Surprised);
                if (Aa != null) yield return (ExpressionPreset.aa, Aa);
                if (Ih != null) yield return (ExpressionPreset.ih, Ih);
                if (Ou != null) yield return (ExpressionPreset.ou, Ou);
                if (Ee != null) yield return (ExpressionPreset.ee, Ee);
                if (Oh != null) yield return (ExpressionPreset.oh, Oh);
                if (Blink != null) yield return (ExpressionPreset.blink, Blink);
                if (BlinkLeft != null) yield return (ExpressionPreset.blinkLeft, BlinkLeft);
                if (BlinkRight != null) yield return (ExpressionPreset.blinkRight, BlinkRight);
                if (LookUp != null) yield return (ExpressionPreset.lookUp, LookUp);
                if (LookDown != null) yield return (ExpressionPreset.lookDown, LookDown);
                if (LookLeft != null) yield return (ExpressionPreset.lookLeft, LookLeft);
                if (LookRight != null) yield return (ExpressionPreset.lookRight, LookRight);
                if (Neutral != null) yield return (ExpressionPreset.neutral, Neutral);
                foreach (var clip in CustomClips)
                {
                    if (clip != null)
                    {
                        yield return (ExpressionPreset.custom, clip);
                    }
                }
            }
        }

        public ExpressionKey CreateKey(VRM10Expression clip)
        {
            foreach (var (preset, c) in Clips)
            {
                if (c == clip)
                {
                    return new ExpressionKey(preset, clip.name);
                }
            }

            // not found
            // return default(ExpressionKey);
            throw new KeyNotFoundException();
        }

        public void AddClip(ExpressionPreset preset, VRM10Expression clip)
        {
            switch (preset)
            {
                case ExpressionPreset.happy: Happy = clip; break;
                case ExpressionPreset.angry: Angry = clip; break;
                case ExpressionPreset.sad: Sad = clip; break;
                case ExpressionPreset.relaxed: Relaxed = clip; break;
                case ExpressionPreset.surprised: Surprised = clip; break;
                case ExpressionPreset.aa: Aa = clip; break;
                case ExpressionPreset.ih: Ih = clip; break;
                case ExpressionPreset.ou: Ou = clip; break;
                case ExpressionPreset.ee: Ee = clip; break;
                case ExpressionPreset.oh: Oh = clip; break;
                case ExpressionPreset.blink: Blink = clip; break;
                case ExpressionPreset.blinkLeft: BlinkLeft = clip; break;
                case ExpressionPreset.blinkRight: BlinkRight = clip; break;
                case ExpressionPreset.lookUp: LookUp = clip; break;
                case ExpressionPreset.lookDown: LookDown = clip; break;
                case ExpressionPreset.lookLeft: LookLeft = clip; break;
                case ExpressionPreset.lookRight: LookRight = clip; break;
                case ExpressionPreset.neutral: Neutral = clip; break;
                default: CustomClips.Add(clip); break;
            }
        }

        public void Replace(IDictionary<VRM10Expression, VRM10Expression> map)
        {
            foreach (var (k, v) in map)
            {
                Replace(k, v);
            }
        }

        void Replace(VRM10Expression src, VRM10Expression dst)
        {
            if (Happy == src)
            {
                Happy = dst; return;
            }
            if (Angry == src)
            {
                Angry = dst; return;
            }
            if (Sad == src)
            {
                Sad = dst; return;
            }
            if (Relaxed == src)
            {
                Relaxed = dst; return;
            }
            if (Surprised == src)
            {
                Surprised = dst; return;
            }
            if (Aa == src)
            {
                Aa = dst; return;
            }
            if (Ih == src)
            {
                Ih = dst; return;
            }
            if (Ou == src)
            {
                Ou = dst; return;
            }
            if (Ee == src)
            {
                Ee = dst; return;
            }
            if (Oh == src)
            {
                Oh = dst; return;
            }
            if (Blink == src)
            {
                Blink = dst; return;
            }
            if (BlinkLeft == src)
            {
                BlinkLeft = dst; return;
            }
            if (BlinkRight == src)
            {
                BlinkRight = dst; return;
            }
            if (LookUp == src)
            {
                LookUp = dst; return;
            }
            if (LookDown == src)
            {
                LookDown = dst; return;
            }
            if (LookLeft == src)
            {
                LookLeft = dst; return;
            }
            if (LookRight == src)
            {
                LookRight = dst; return;
            }
            if (Neutral == src)
            {
                Neutral = dst; return;
            }

            for (int i = 0; i < CustomClips.Count; ++i)
            {
                if (CustomClips[i] == src)
                {
                    CustomClips[i] = dst;
                    return;
                }
            }
        }

        /// <summary>
        /// NullのClipを削除して詰める
        /// </summary>
        public void RemoveNullClip()
        {
            if (CustomClips == null)
            {
                return;
            }
            for (int i = CustomClips.Count - 1; i >= 0; --i)
            {
                if (CustomClips[i] == null)
                {
                    CustomClips.RemoveAt(i);
                }
            }
        }

        // /// <summary>
        // /// Unknown以外で存在しないものを全て作る
        // /// </summary>
        // public void CreateDefaultPreset()
        // {
        //     foreach (var preset in ((ExpressionPreset[])Enum.GetValues(typeof(ExpressionPreset)))
        //         .Where(x => x != ExpressionPreset.custom)
        //         )
        //     {
        //         CreateDefaultPreset(preset);
        //     }
        // }



    }
}
