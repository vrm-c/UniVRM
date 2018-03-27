using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace VRM
{
    public class VRMBlendShapeProxy : MonoBehaviour
    {
        [SerializeField]
        public BlendShapeAvatar BlendShapeAvatar;

        Dictionary<BlendShapeKey, BlendShapeClipHandler> m_handlerMap;
        public Dictionary<BlendShapeKey, BlendShapeClipHandler> CreateHandlerMap()
        {
            var handlerMap = new Dictionary<BlendShapeKey, BlendShapeClipHandler>(
            new BlendShapeKey.CustomerEqualityComparer());
            foreach (var x in BlendShapeAvatar.Clips)
            {
                handlerMap.Add(BlendShapeKey.CreateFrom(x), new BlendShapeClipHandler(x, transform));
                ;
            }
            return handlerMap;
        }

        private void Update()
        {
            if (BlendShapeAvatar != null)
            {
                if (m_handlerMap == null)
                {
                    m_handlerMap = CreateHandlerMap();
                }
            }
        }

        public void Reload()
        {
            if (BlendShapeAvatar == null) return;

            foreach (var kv in m_handlerMap)
            {
                SetValue(kv.Key, GetValue(kv.Key));
            }
        }

        public void SetValue(BlendShapePreset key, float value)
        {
            SetValue(new BlendShapeKey(key), value);
        }
        public float GetValue(BlendShapePreset key)
        {
            return GetValue(new BlendShapeKey(key));
        }
        public void SetValue(String key, float value)
        {
            SetValue(new BlendShapeKey(key), value);
        }
        public float GetValue(String key)
        {
            return GetValue(new BlendShapeKey(key));
        }
        public void SetValue(BlendShapeKey key, float value)
        {
            if (m_handlerMap == null) return;
            BlendShapeClipHandler handler;
            if (m_handlerMap.TryGetValue(key, out handler))
            {
                handler.Apply(value);
            }
        }
        public float GetValue(BlendShapeKey key)
        {
            if (m_handlerMap == null) return 0;
            BlendShapeClipHandler handler;
            if (!m_handlerMap.TryGetValue(key, out handler))
            {
                return 0;
            }
            return handler.LastValue;
        }

        public void ClearKeys()
        {
            if (m_handlerMap == null) return;
            foreach(var kv in m_handlerMap)
            {
                kv.Value.Apply(0);
            }
        }
    }
}
