using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    delegate (string, MessageType) VRM10MetaPropertyValidator(SerializedProperty prop);

    class VRM10MetaProperty
    {
        public SerializedProperty m_prop;

        VRM10MetaPropertyValidator m_validator;

        public VRM10MetaProperty(SerializedProperty prop,
            VRM10MetaPropertyValidator validator = null)
        {
            m_prop = prop;
            if (validator == null)
            {
                // no validation
                validator = _ => ("", MessageType.None);
            }
            m_validator = validator;
        }

        public void OnGUI()
        {
            // var old = m_prop.stringValue;
            if (m_prop.propertyType == SerializedPropertyType.Generic)
            {
                if (m_prop.arrayElementType != null)
                {
                    EditorGUILayout.LabelField(m_prop.name);

                    var depth = m_prop.depth;
                    var iterator = m_prop.Copy();
                    var arrayName = iterator.name;
                    for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
                    {
                        if (iterator.depth < depth)
                            break;

                        depth = iterator.depth;

                        // using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                        var match = System.Text.RegularExpressions.Regex.Match(iterator.propertyPath, "\\[(\\d+)\\]$");
                        if (match != null && iterator.type == "string")
                        {
                            // ArrayItem
                            // Debug.Log($"{match.Groups[1].Value}");
                            iterator.stringValue = EditorGUILayout.TextField($"  {arrayName}[{match.Groups[1].Value}]", iterator.stringValue);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(iterator, true);
                        }
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            else
            {
                EditorGUILayout.PropertyField(m_prop);
            }
            var (msg, msgType) = m_validator(m_prop);
            if (!string.IsNullOrEmpty(msg))
            {
                EditorGUILayout.HelpBox(msg, msgType);
            }
            // return old != m_prop.stringValue;
        }
    }
}