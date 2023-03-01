using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMShaders
{
    public class AnimationClipFactory : IResponsibilityForDestroyObjects
    {
        private readonly IReadOnlyDictionary<SubAssetKey, AnimationClip> _externalClips;
        private readonly Dictionary<SubAssetKey, AnimationClip> _runtimeClips = new Dictionary<SubAssetKey, AnimationClip>();
        private readonly List<SubAssetKey> _loadedClipKeys = new List<SubAssetKey>();

        /// <summary>
        /// 外部アセットとして渡された AnimationClip
        /// </summary>
        public IReadOnlyDictionary<SubAssetKey, AnimationClip> ExternalClips => _externalClips;

        /// <summary>
        /// ImporterContext によって Runtime に生成された AnimationClip
        /// </summary>
        public IReadOnlyDictionary<SubAssetKey, AnimationClip> RuntimeGeneratedClips => _runtimeClips;

        /// <summary>
        /// ImporterContext によって必要とされた AnimationClip の SubAssetKey.
        /// 必ずしも ExternalClips と RuntimeGeneratedClips の集合とは限らない.
        /// </summary>
        public IReadOnlyList<SubAssetKey> LoadedClipKeys => _loadedClipKeys;

        public AnimationClipFactory(IReadOnlyDictionary<SubAssetKey, AnimationClip> externalClips)
        {
            _externalClips = externalClips;
        }

        public void Dispose()
        {
            foreach (var kv in _runtimeClips)
            {
                UnityObjectDestroyer.DestroyRuntimeOrEditor(kv.Value);
            }
            _runtimeClips.Clear();
        }

        public void TransferOwnership(TakeResponsibilityForDestroyObjectFunc take)
        {
            foreach (var (key, o) in _runtimeClips.ToArray())
            {
                take(key, o);
                _runtimeClips.Remove(key);
            }
        }

        public AnimationClip GetAnimationClip(SubAssetKey key)
        {
            if (_externalClips.TryGetValue(key, out var clip))
            {
                return clip;
            }

            if (_runtimeClips.TryGetValue(key, out clip))
            {
                return clip;
            }

            return null;
        }

        public async Task<AnimationClip> LoadAnimationClipAsync(SubAssetKey key, Func<Task<AnimationClip>> loadAnimationClip)
        {
            if (!_loadedClipKeys.Contains(key))
            {
                _loadedClipKeys.Add(key);
            }

            var clip = GetAnimationClip(key);
            if (clip != null)
            {
                return clip;
            }

            clip = await loadAnimationClip();
            _runtimeClips.Add(key, clip);
            return clip;
        }
    }
}
