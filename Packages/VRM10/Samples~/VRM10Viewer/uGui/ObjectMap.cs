using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    class ObjectMap
    {
        Dictionary<string, GameObject> _map = new();
        public IReadOnlyDictionary<string, GameObject> Objects => _map;

        public ObjectMap(GameObject root)
        {
            foreach (var x in root.GetComponentsInChildren<Transform>())
            {
                _map[x.name] = x.gameObject;
            }
        }

        public T Get<T>(string name) where T : Component
        {
            return _map[name].GetComponent<T>();
        }
    }
}