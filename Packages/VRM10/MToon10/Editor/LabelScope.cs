using System;
using UnityEditor;
using UnityEngine;

namespace VRM10.MToon10.Editor
{
    public readonly struct LabelScope : IDisposable
    {
        public LabelScope(string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
        }
        
        public void Dispose()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}