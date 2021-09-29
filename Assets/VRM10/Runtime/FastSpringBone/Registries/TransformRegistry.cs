using System;
using System.Collections.Generic;
using VRM.FastSpringBones.NativeWrappers;

namespace VRM.FastSpringBones.Registries
{
    /// <summary>
    /// 今生きているTransformの一覧を返すクラス
    /// </summary>
    public sealed class TransformRegistry
    {
        private readonly List<NativeTransform> _transforms = new List<NativeTransform>();
        public IReadOnlyList<NativeTransform> Transforms => _transforms;

        private readonly List<NativeTransform> _pullTargets = new List<NativeTransform>();
        public IReadOnlyList<NativeTransform> PullTargets => _pullTargets;

        private readonly List<NativeTransform> _pushTargets = new List<NativeTransform>();
        public IReadOnlyList<NativeTransform> PushTargets => _pushTargets;

        private Action _onValueChanged;
        
        public void SubscribeOnValueChanged(Action action) => _onValueChanged += action;
        
        public void UnSubscribeOnValueChanged(Action action) => _onValueChanged -= action;
        
        public void Register(NativeTransform nativeTransform, TransformSynchronizationType synchronizationType)
        {
            _transforms.Add(nativeTransform);
            switch (synchronizationType)
            {
                case TransformSynchronizationType.PullOnly:
                    _pullTargets.Add(nativeTransform);
                    break;
                case TransformSynchronizationType.PushOnly:
                    _pushTargets.Add(nativeTransform);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(synchronizationType), synchronizationType, null);
            }
            _onValueChanged?.Invoke();
        }

        public void Unregister(NativeTransform nativeTransform)
        {
            _transforms.Remove(nativeTransform);

            if (_pullTargets.Contains(nativeTransform))
            {
                _pullTargets.Remove(nativeTransform);
            }

            if (_pushTargets.Contains(nativeTransform))
            {
                _pushTargets.Remove(nativeTransform);
            }
            _onValueChanged?.Invoke();
        }
    }
}