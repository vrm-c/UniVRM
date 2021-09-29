using System;
using System.Collections.Generic;

namespace VRM.FastSpringBones.Registries
{
    public class Registry<T>
    {
        private readonly List<T> _items = new List<T>();
        private Action _onValueChanged;

        public IReadOnlyList<T> Items => _items;

        public void Register(T value)
        {
            _items.Add(value);
            _onValueChanged?.Invoke();
        }

        public void Unregister(T value)
        {
            _items.Remove(value);
            _onValueChanged?.Invoke();
        }

        public void SubscribeOnValueChanged(Action action) => _onValueChanged += action;
        public void UnSubscribeOnValueChanged(Action action) => _onValueChanged -= action;
    }
}
